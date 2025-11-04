# Limitations Remediation Plan

**Date**: December 2025  
**Status**: Planning Phase  
**Based On**: Test Implementation Findings

---

## Executive Summary

During comprehensive test implementation, several limitations were discovered in the current codebase. This document outlines a plan to address these limitations systematically, prioritizing based on impact and complexity.

---

## Discovered Limitations

### 1. Cancellation Token Support (Not Implemented)
**Severity**: Medium  
**Impact**: Users cannot cancel long-running operations  
**Location**: `CsvAdapter` - All async methods

**Current State**:
- Methods accept `CancellationToken` parameter
- Token is never checked after `Task.Yield()`
- No cancellation support for long-running file I/O operations

**Test Evidence**: `CsvAdapter_ListAsync_WithCancellation_AcceptsCancellationToken` documents this limitation

---

### 2. Duplicate Field Handling in SelectFields
**Severity**: Low  
**Impact**: Users cannot request duplicate fields in field projection  
**Location**: `CsvAdapter.SelectFields()` method

**Current State**:
- `SelectFields` throws `ArgumentException` when duplicate field names are provided
- No deduplication logic
- Example: `Fields = ["name", "name", "email"]` throws exception

**Test Evidence**: `CsvAdapter_ListAsync_SelectFields_WithDuplicateFields_ThrowsException`

---

### 3. Null Value Handling in CSV Format
**Severity**: Low (Design Decision)  
**Impact**: Null values are converted to empty strings (CSV format limitation)  
**Location**: CSV read/write operations

**Current State**:
- CSV format inherently doesn't support null values
- Null values are converted to empty strings during write
- Reads return empty strings, not null

**Test Evidence**: `CsvAdapter_UpdateAsync_WithNullValue_ConvertsToEmptyString`

**Note**: This may be acceptable behavior depending on requirements. Consider documenting as feature rather than limitation.

---

### 4. Concurrent Write Locking
**Severity**: Medium  
**Impact**: Concurrent writes may fail with IOException  
**Location**: `CsvAdapter.CreateAsync()` and file locking mechanism

**Current State**:
- File locking prevents concurrent writes
- Multiple simultaneous writes cause `IOException`
- No retry mechanism
- No queue/batching for concurrent operations

**Test Evidence**: `CsvAdapter_ConcurrentWrites_MayRequireRetry`

---

### 5. New Field Persistence in Updates
**Severity**: Medium  
**Impact**: New fields added via UpdateAsync may not persist properly  
**Location**: `CsvAdapter.UpdateAsync()` and CSV header management

**Current State**:
- `UpdateAsync` adds new fields to in-memory record
- CSV headers are not updated when new fields are added
- New fields may be lost when file is rewritten
- Only existing header fields are preserved

**Test Evidence**: `CsvAdapter_UpdateAsync_WithNewField_AddsFieldToRecord` (currently just verifies method completes)

---

## Remediation Plan

### Phase 1: High Priority Fixes (Immediate)

#### 1.1 Implement Cancellation Token Support
**Priority**: High  
**Effort**: Medium (2-3 days)  
**Risk**: Low

**Approach**:
1. Add cancellation token checks after `Task.Yield()` in all async methods
2. Check token before file I/O operations
3. Check token in loops (if any)
4. Use `ct.ThrowIfCancellationRequested()` for explicit checks
5. Ensure file locks are released on cancellation

**Implementation Steps**:
```
1. Update ListAsync:
   - Add ct.ThrowIfCancellationRequested() after Task.Yield()
   - Check before file read operations
   - Check in filter/sort loops if processing large datasets

2. Update GetAsync:
   - Add ct.ThrowIfCancellationRequested() after Task.Yield()
   - Check before file read

3. Update CreateAsync:
   - Add ct.ThrowIfCancellationRequested() after Task.Yield()
   - Check before file write
   - Ensure lock is released on cancellation

4. Update UpdateAsync:
   - Add ct.ThrowIfCancellationRequested() after Task.Yield()
   - Check before file read/write
   - Ensure lock is released on cancellation

5. Update DeleteAsync:
   - Add ct.ThrowIfCancellationRequested() after Task.Yield()
   - Check before file read/write
   - Ensure lock is released on cancellation

6. Update GetSchemaAsync:
   - Add ct.ThrowIfCancellationRequested() after Task.Yield()
   - Check before file read

7. Update ListCollectionsAsync:
   - Add ct.ThrowIfCancellationRequested() after Task.Yield()
   - Check before directory enumeration
```

**Testing Strategy**:
- Update existing cancellation tests to verify actual cancellation
- Add tests for cancellation during file I/O
- Add tests for lock release on cancellation
- Test cancellation propagation

**Files to Modify**:
- `DataAbstractionAPI.Adapters.Csv/CsvAdapter.cs`

---

#### 1.2 Fix Duplicate Field Handling
**Priority**: Medium  
**Effort**: Low (1 day)  
**Risk**: Low

**Approach**:
1. Deduplicate field array in `SelectFields` method
2. Use `Distinct()` or similar to remove duplicates
3. Preserve order (first occurrence)
4. Log warning if duplicates detected (optional)

**Implementation Steps**:
```
1. Update SelectFields method:
   - Add deduplication: fields = fields.Distinct().ToArray()
   - Or use HashSet for deduplication while preserving order
   - Consider logging warning for duplicate fields

2. Update test to verify:
   - Duplicates are removed
   - Order is preserved (first occurrence)
   - No exception is thrown
```

**Testing Strategy**:
- Update existing duplicate field test
- Test with multiple duplicates
- Test order preservation
- Verify no performance impact

**Files to Modify**:
- `DataAbstractionAPI.Adapters.Csv/CsvAdapter.cs`
- `DataAbstractionAPI.Adapters.Tests/CsvAdapterTests.cs`

---

### Phase 2: Medium Priority Fixes (Short Term)

#### 2.1 Implement Concurrent Write Retry Logic
**Priority**: Medium  
**Effort**: Medium (2-3 days)  
**Risk**: Medium

**Approach**:
1. Implement exponential backoff retry mechanism
2. Add configurable retry attempts and delays
3. Distinguish between lock timeout and other IO errors
4. Consider using semaphore or queue for write operations

**Implementation Options**:

**Option A: Retry with Exponential Backoff** (Recommended)
```
- Wrap file write operations in retry logic
- Retry on IOException (file locked)
- Use exponential backoff: 50ms, 100ms, 200ms, 400ms
- Max retries: 3-5 attempts
- Throw exception after max retries
```

**Option B: Write Queue** (More Complex)
```
- Implement internal queue for write operations
- Process writes sequentially
- Return immediately with queued task
- More complex but handles high concurrency better
```

**Recommendation**: Start with Option A (simpler, sufficient for most cases)

**Implementation Steps**:
```
1. Create RetryHelper utility class:
   - RetryAsync<T>(Func<Task<T>> operation, int maxRetries, int baseDelayMs)
   - Handle IOException specifically
   - Exponential backoff logic

2. Update CreateAsync:
   - Wrap AppendRecord call in retry logic
   - Configure retry parameters

3. Update UpdateAsync:
   - Wrap file write in retry logic

4. Update DeleteAsync:
   - Wrap file write in retry logic

5. Consider making retry configurable:
   - Add to CsvAdapter constructor options
   - Or use appsettings.json configuration
```

**Testing Strategy**:
- Update concurrent write test to verify retries work
- Test retry exhaustion (all retries fail)
- Test retry success after initial failures
- Test non-retryable errors (not IOException)
- Performance test with high concurrency

**Files to Create**:
- `DataAbstractionAPI.Adapters.Csv/RetryHelper.cs` (optional utility)

**Files to Modify**:
- `DataAbstractionAPI.Adapters.Csv/CsvAdapter.cs`
- `DataAbstractionAPI.Adapters.Tests/CsvAdapterTests.cs`

---

#### 2.2 Fix New Field Persistence in Updates
**Priority**: Medium  
**Effort**: Medium-High (3-4 days)  
**Risk**: Medium

**Approach**:
1. Update CSV headers when new fields are added
2. Ensure all records have new field (with empty string for existing records)
3. Preserve column order (append new fields to end)
4. Handle schema file updates if using schema files

**Implementation Steps**:
```
1. Update UpdateAsync method:
   - Detect new fields in update dictionary
   - Read current headers from file
   - Identify new fields not in headers
   - If new fields exist:
     a. Read all records
     b. Add new fields to headers (append to end)
     c. Add empty string value for new fields in existing records
     d. Write updated headers and all records
     e. Update schema file if exists

2. Consider helper method:
   - UpdateHeadersIfNeeded(Collection<string> newFields)
   - Returns updated header list
   - Handles header file update

3. Update CsvFileHandler:
   - Add method to update headers
   - Ensure thread-safe header updates
   - Consider header versioning/locking

4. Update CsvSchemaManager (if used):
   - Update schema JSON when headers change
   - Maintain field definitions for new fields
```

**Edge Cases to Handle**:
- Multiple concurrent updates adding different fields
- Update that adds field, then another update before first completes
- Schema file consistency
- Performance with large datasets (many records)

**Testing Strategy**:
- Test adding single new field
- Test adding multiple new fields
- Test concurrent updates adding different fields
- Test that existing records get empty string for new field
- Test header order preservation
- Test schema file updates
- Performance test with large datasets

**Files to Modify**:
- `DataAbstractionAPI.Adapters.Csv/CsvAdapter.cs`
- `DataAbstractionAPI.Adapters.Csv/CsvFileHandler.cs`
- `DataAbstractionAPI.Adapters.Csv/CsvSchemaManager.cs` (if applicable)
- `DataAbstractionAPI.Adapters.Tests/CsvAdapterTests.cs`

---

### Phase 3: Low Priority / Design Decisions (Long Term)

#### 3.1 Null Value Handling (Design Decision)
**Priority**: Low  
**Effort**: N/A (Design Decision)  
**Risk**: N/A

**Decision Required**: Is current behavior acceptable?

**Options**:
1. **Keep current behavior** (recommended)
   - Document as CSV format limitation
   - Empty string represents null
   - Consider adding helper method to distinguish null from empty

2. **Implement null support**
   - Use special marker (e.g., `\N` or `NULL`)
   - Parse on read to convert back to null
   - Requires schema changes
   - Breaking change for existing data

**Recommendation**: Keep current behavior, document clearly

**Action Items**:
- [ ] Document null handling in API documentation
- [ ] Add helper method `IsNullValue(string value)` if needed
- [ ] Update test documentation to clarify this is expected behavior

---

#### 3.2 Enhanced Concurrency Support (Future Enhancement)
**Priority**: Low  
**Effort**: High (1-2 weeks)  
**Risk**: High

**Future Enhancement Ideas**:
1. Write queue with background processing
2. Optimistic concurrency control
3. Transaction support for multi-record operations
4. Distributed locking for multi-instance scenarios

**Recommendation**: Defer to Phase 4 or future major version

---

## Implementation Timeline

### Week 1: High Priority Fixes
- **Day 1-2**: Implement cancellation token support
- **Day 3**: Fix duplicate field handling
- **Day 4-5**: Testing and bug fixes

### Week 2: Medium Priority Fixes
- **Day 1-2**: Implement concurrent write retry logic
- **Day 3-4**: Fix new field persistence in updates
- **Day 5**: Integration testing

### Week 3: Testing & Documentation
- **Day 1-2**: Comprehensive testing
- **Day 3**: Performance testing
- **Day 4-5**: Documentation updates

---

## Testing Strategy

### Unit Tests
- Update existing tests that document limitations
- Add new tests for fixes
- Test edge cases and error scenarios

### Integration Tests
- Test cancellation propagation through API layer
- Test concurrent operations under load
- Test field persistence across operations

### Performance Tests
- Benchmark cancellation overhead
- Test retry mechanism performance
- Test header update performance with large datasets

### Regression Tests
- Ensure all existing tests still pass
- Verify no breaking changes
- Test backward compatibility

---

## Risk Assessment

### Low Risk
- ✅ Cancellation token support (well-understood pattern)
- ✅ Duplicate field handling (simple deduplication)

### Medium Risk
- ⚠️ Concurrent write retry (may introduce performance issues)
- ⚠️ New field persistence (complex logic, may affect performance)

### Mitigation Strategies
1. Feature flags for new behavior (allow rollback)
2. Comprehensive testing before deployment
3. Performance monitoring
4. Gradual rollout if possible
5. Backup/rollback plan

---

## Success Criteria

### Cancellation Token Support
- [ ] All async methods check cancellation token
- [ ] Cancellation works during file I/O
- [ ] Locks are released on cancellation
- [ ] Tests verify actual cancellation

### Duplicate Field Handling
- [ ] Duplicate fields are deduplicated
- [ ] Order is preserved (first occurrence)
- [ ] No exceptions thrown
- [ ] Performance impact is minimal

### Concurrent Write Retry
- [ ] Retry logic handles file locking
- [ ] Configurable retry parameters
- [ ] Distinguishes retryable vs non-retryable errors
- [ ] Performance is acceptable

### New Field Persistence
- [ ] New fields added via UpdateAsync persist
- [ ] Headers are updated correctly
- [ ] Existing records get empty string for new fields
- [ ] Schema files are updated if used
- [ ] Performance is acceptable with large datasets

---

## Documentation Updates Required

1. **API Documentation**
   - Document cancellation token support
   - Document null value handling (CSV limitation)
   - Document concurrent write behavior

2. **Code Comments**
   - Document cancellation token usage
   - Document retry logic
   - Document header update logic

3. **Test Documentation**
   - Update test comments for fixed limitations
   - Document new test scenarios

4. **User Guide**
   - Explain cancellation behavior
   - Explain concurrent write limitations
   - Explain null value handling

---

## Dependencies & Prerequisites

### For Cancellation Token Support
- No external dependencies
- Requires understanding of async/await patterns

### For Concurrent Write Retry
- No external dependencies
- May benefit from retry library (Polly) but not required

### For New Field Persistence
- No external dependencies
- Requires careful handling of CSV header updates

---

## Open Questions & Recommendations

### 1. Cancellation Token Support at API Level
**Question**: Should we support cancellation at API level? (Currently only at adapter level)

**Recommendation**: ✅ **YES - Support at API level with proper propagation**

**Reasoning**:
- **User Experience**: HTTP clients can cancel requests (browser navigation, timeout, user action). This should propagate through the entire stack.
- **ASP.NET Core Best Practice**: ASP.NET Core automatically provides `HttpContext.RequestAborted` cancellation token. We should use it.
- **Resource Cleanup**: Cancellation at API level ensures resources are freed even if request is cancelled mid-operation.
- **Minimal Effort**: Controller methods already accept `CancellationToken` implicitly via `HttpContext.RequestAborted`.

**Implementation Approach**:
```
1. Update DataController methods to accept CancellationToken parameter:
   - GetCollection(string collection, [FromQuery] int? limit, CancellationToken ct)
   - GetRecord(string collection, string id, CancellationToken ct)
   - etc.

2. ASP.NET Core automatically binds HttpContext.RequestAborted to CancellationToken
   - No additional configuration needed
   - Works out of the box

3. Pass cancellation token to adapter methods:
   - await _adapter.ListAsync(collection, options, ct);
```

**Benefits**:
- ✅ Standard ASP.NET Core pattern
- ✅ Automatic cancellation on client disconnect
- ✅ Proper resource cleanup
- ✅ Better user experience

**Trade-offs**:
- Slightly more verbose controller methods (but standard practice)
- Need to ensure all adapter calls pass token (easy to verify)

---

### 2. Retry Configuration
**Question**: Should retry parameters be configurable per-adapter or globally?

**Recommendation**: ✅ **Per-Adapter with Sensible Defaults**

**Reasoning**:
- **Flexibility**: Different adapters may have different retry needs (CSV vs. SQL vs. NoSQL)
- **Use Case Diversity**: Some deployments may need aggressive retries (high concurrency), others may prefer fast failure
- **Simple Defaults**: Most users won't need to configure - sensible defaults work for 90% of cases
- **Configuration Over Code**: Make it easy to configure via constructor or options class

**Implementation Approach**:
```
1. Create RetryOptions class:
   public class RetryOptions
   {
       public int MaxRetries { get; set; } = 3;
       public int BaseDelayMs { get; set; } = 50;
       public bool Enabled { get; set; } = true;
   }

2. Add to CsvAdapter constructor:
   public CsvAdapter(
       string baseDirectory, 
       IDefaultGenerator? defaultGenerator = null,
       ITypeConverter? typeConverter = null,
       RetryOptions? retryOptions = null)

3. Use defaults if not provided:
   _retryOptions = retryOptions ?? new RetryOptions();

4. Future: Support appsettings.json configuration:
   "CsvAdapter": {
     "RetryOptions": {
       "MaxRetries": 5,
       "BaseDelayMs": 100
     }
   }
```

**Benefits**:
- ✅ Flexible per-adapter configuration
- ✅ Sensible defaults (no config needed for most cases)
- ✅ Easy to override when needed
- ✅ Testable (can inject different retry options)

**Trade-offs**:
- More constructor parameters (but optional with defaults)
- Slightly more complex than global config

**Alternative Considered**: Global configuration
- **Rejected because**: Different adapters have different needs. A global config is too rigid.

---

### 3. New Field Defaults
**Question**: Should new fields use intelligent defaults (from DefaultGenerator) or just empty strings?

**Recommendation**: ✅ **Use Intelligent Defaults (DefaultGenerator) when available, fallback to empty string**

**Reasoning**:
- **Consistency**: DefaultGenerator already exists and is designed for this purpose
- **Better UX**: Intelligent defaults (e.g., `created_at` → current timestamp, `is_active` → false) are more useful than empty strings
- **Pattern Recognition**: Many new fields follow naming patterns that DefaultGenerator can handle
- **Type Awareness**: DefaultGenerator can provide type-appropriate defaults
- **Optional**: If DefaultGenerator is not injected, fall back to empty string (backward compatible)

**Implementation Approach**:
```
1. When new field is added in UpdateAsync:
   - Check if DefaultGenerator is available
   - If yes:
     a. Determine field type (infer from first value or use String as default)
     b. Create DefaultGenerationContext with collection name
     c. Call _defaultGenerator.GenerateDefault(fieldName, fieldType, context)
     d. Use generated default for existing records
   - If no:
     a. Use empty string (current behavior)

2. For new field's first value:
   - Use the value provided in the update
   - Apply intelligent default to all OTHER existing records

3. Example:
   Update adds field "is_verified" (Boolean type)
   - DefaultGenerator returns: false
   - Existing records get: false
   - Updated record gets: true (from update)
```

**Benefits**:
- ✅ Leverages existing DefaultGenerator infrastructure
- ✅ Better default values for users
- ✅ Type-aware defaults
- ✅ Backward compatible (works without DefaultGenerator)

**Trade-offs**:
- Requires DefaultGenerator to be injected (but it's optional)
- Need to infer field type (may default to String if unclear)
- Slightly more complex logic

**Alternative Considered**: Always use empty string
- **Rejected because**: Wastes the DefaultGenerator capability that's already built. Empty strings are less useful than intelligent defaults.

**Note**: This aligns with the spec's "Intelligent Default Generation" feature mentioned in `data-abstraction-api.md`.

---

### 4. Schema File Consistency
**Question**: How should schema files be kept in sync with CSV headers?

**Recommendation**: ✅ **CSV Headers as Source of Truth, Schema Files as Optional Metadata**

**Reasoning**:
- **Simplicity**: CSV headers are always present and represent the actual data structure
- **Reliability**: Schema files can become stale or be deleted - CSV headers can't
- **Incremental Adoption**: Schema files can be added later without breaking existing functionality
- **Use Case**: Schema files are useful for metadata (field types, constraints, descriptions) that CSV headers don't contain

**Implementation Approach**:
```
1. CSV Headers are Primary:
   - GetSchemaAsync() reads headers from CSV file
   - Headers always reflect current structure
   - Schema files are not required

2. Schema Files are Optional Metadata:
   - When headers change, update schema file if it exists
   - Schema file can contain additional metadata:
     - Field types (inferred or explicit)
     - Constraints
     - Descriptions
     - Default values
   
3. Update Strategy:
   - When UpdateAsync adds new fields:
     a. Update CSV headers (required)
     b. If schema file exists:
        - Load existing schema
        - Add new field definitions
        - Infer type from first value
        - Use DefaultGenerator for default value
        - Save updated schema
   
4. Schema File Structure Enhancement:
   {
     "name": "users",
     "fields": [
       {
         "name": "id",
         "type": "string",
         "nullable": false,
         "default": null,
         "description": "Unique identifier"
       },
       {
         "name": "created_at",
         "type": "datetime",
         "nullable": false,
         "default": "current_timestamp",
         "description": "Record creation timestamp"
       }
     ]
   }

5. GetSchemaAsync Logic:
   - Read headers from CSV (always)
   - If schema file exists:
     - Load schema file
     - Merge: Use header order from CSV, enrich with metadata from schema
     - For fields in CSV but not in schema: infer type from data
   - If no schema file:
     - Create schema from headers only (infer types)
```

**Benefits**:
- ✅ CSV headers are always accurate (source of truth)
- ✅ Schema files provide rich metadata when available
- ✅ Works without schema files (backward compatible)
- ✅ Schema files can be added incrementally
- ✅ No risk of schema/header mismatch

**Trade-offs**:
- Need to merge CSV headers with schema metadata (but straightforward)
- Schema files may lag slightly (but CSV is always correct)

**Alternative Considered**: Schema files as source of truth
- **Rejected because**: Too risky - schema files can be deleted, corrupted, or out of sync. CSV headers are always present.

**Alternative Considered**: Always keep in sync (bidirectional)
- **Rejected because**: Too complex. CSV headers should be primary - they represent actual data structure.

---

### 5. Performance Targets
**Question**: What are acceptable performance targets for header updates with large datasets?

**Recommendation**: ✅ **Pragmatic Performance Targets with Optimization Options**

**Reasoning**:
- **Real-World Usage**: Most CSV files are small to medium (< 100K records)
- **Optimize for Common Case**: Most collections will have < 10K records
- **Acceptable Degradation**: Header updates are infrequent operations
- **Optimization Path**: Can optimize later if needed, but start simple

**Performance Targets**:
```
Small (< 1,000 records):  < 100ms
Medium (1,000 - 10,000):  < 500ms
Large (10,000 - 100,000): < 2 seconds
Very Large (> 100,000):   < 5 seconds (acceptable for infrequent operation)

Optimization Threshold:
- If header update takes > 5 seconds, consider:
  - Streaming approach (read/write in chunks)
  - Background processing
  - Batch operations
```

**Implementation Approach**:
```
1. Start with Simple Approach:
   - Read all records into memory
   - Add new field to all records
   - Write all records back
   - This works fine for < 100K records

2. Add Performance Monitoring:
   - Log warning if operation takes > 2 seconds
   - Consider adding timing metrics

3. Future Optimization (if needed):
   - Stream processing for very large files
   - Incremental updates (append-only for new fields)
   - Background processing for large operations
```

**Benefits**:
- ✅ Simple implementation initially
- ✅ Works for 99% of use cases
- ✅ Clear optimization path if needed
- ✅ Measurable targets

**Trade-offs**:
- Large files (> 100K records) may be slow
- But header updates are infrequent, so acceptable

**Note**: Most real-world CSV collections are small. If performance becomes an issue, optimize then.

---

## Summary of Recommendations

| Decision | Recommendation | Priority | Effort |
|----------|---------------|----------|--------|
| **Cancellation at API Level** | ✅ YES - Support with HttpContext.RequestAborted | High | Low |
| **Retry Configuration** | ✅ Per-Adapter with defaults | Medium | Low |
| **New Field Defaults** | ✅ Use DefaultGenerator when available | Medium | Medium |
| **Schema Consistency** | ✅ CSV headers primary, schema files optional metadata | High | Medium |
| **Performance Targets** | ✅ Pragmatic targets, optimize later if needed | Low | Low |

---

## Implementation Priority Based on Recommendations

1. **High Priority** (Week 1):
   - Cancellation at API level (easy win, high value)
   - Schema consistency approach (foundational decision)

2. **Medium Priority** (Week 2):
   - Intelligent defaults for new fields (uses existing infrastructure)
   - Retry configuration (straightforward)

3. **Low Priority** (As needed):
   - Performance optimization (only if real-world usage shows issues)

---

## Next Steps

1. **Review this plan** with team/stakeholders
2. **Prioritize** based on business needs
3. **Create tickets/issues** for each phase
4. **Assign resources** and set timeline
5. **Begin implementation** with Phase 1

---

## Appendix: Related Files

### Files to Modify
- `DataAbstractionAPI.Adapters.Csv/CsvAdapter.cs`
- `DataAbstractionAPI.Adapters.Csv/CsvFileHandler.cs`
- `DataAbstractionAPI.Adapters.Csv/CsvSchemaManager.cs` (possibly)
- `DataAbstractionAPI.Adapters.Tests/CsvAdapterTests.cs`

### Files to Create (Optional)
- `DataAbstractionAPI.Adapters.Csv/RetryHelper.cs`
- `DataAbstractionAPI.Adapters.Csv/RetryOptions.cs`

### Documentation to Update
- `README.md`
- `API_USAGE.md`
- `TEST_IMPLEMENTATION_SUMMARY.md`
- Code XML comments

---

**Document Status**: Ready for Review  
**Last Updated**: December 2025

