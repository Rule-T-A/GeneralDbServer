# Services Layer Test Coverage Plan

**Goal**: Achieve >85% test coverage for DataAbstractionAPI.Services  
**Status**: ⏸️ PENDING  
**Current Tests**: 89 tests (13 DefaultGenerator + 21 TypeConverter + 24 FilterEvaluator + 19 ValidationService + 12 Integration)

---

## Overview

This plan outlines the steps to measure, analyze, and improve test coverage for the Services layer to meet the >85% requirement specified in Phase 2 of the implementation plan.

---

## Step 1: Measure Current Coverage

**Effort**: 30 minutes  
**Priority**: High

### 1.1: Run Coverage Analysis

```bash
# Run tests with coverage collection
dotnet test DataAbstractionAPI.Services.Tests /p:CollectCoverage=true /p:CoverletOutputFormat=opencover /p:CoverletOutput=./coverage/

# Generate HTML report (requires ReportGenerator package)
dotnet tool install -g dotnet-reportgenerator-globaltool
reportgenerator -reports:./coverage/coverage.opencover.xml -targetdir:./coverage/html -reporttypes:Html
```

### 1.2: Analyze Coverage Report

- [ ] Review coverage percentage for each service class
- [ ] Identify uncovered lines/branches
- [ ] Document coverage gaps per service:
  - DefaultGenerator: ___%
  - TypeConverter: ___%
  - FilterEvaluator: ___%
  - ValidationService: ___%
  - Overall: ___%

### 1.3: Document Baseline

- [ ] Create coverage baseline report
- [ ] Identify critical paths with low coverage
- [ ] Prioritize gaps by importance

**Validation**: Coverage report generated, baseline documented

---

## Step 2: DefaultGenerator Coverage Gaps

**Effort**: 2-3 hours  
**Priority**: High

### 2.1: Missing Pattern Tests

Based on code analysis, these patterns need additional tests:

- [ ] **Date Type Patterns**:
  - Test `deleted_at` pattern (DateTime/Date)
  - Test `_date` suffix pattern (DateTime/Date)
  - Test `created_date` pattern
  - Test `updated_date` pattern

- [ ] **Count Patterns**:
  - Test `num_*` prefix pattern (e.g., `num_items`)
  - Test `*_count` with different field types
  - Test `*_total` with Float type

- [ ] **ID Patterns**:
  - Test `*_id` with different field types (Integer, String)
  - Test `*_key` with different field types

### 2.2: Missing Type-Based Default Tests

- [ ] **Array Type**:
  - Test `GenerateTypeBasedDefault` for Array type
  - Verify returns `Array.Empty<object>()`

- [ ] **Object Type**:
  - Test `GenerateTypeBasedDefault` for Object type
  - Verify returns `new Dictionary<string, object>()`

- [ ] **Date Type**:
  - Test `GenerateTypeBasedDefault` for Date type
  - Verify returns `DateTime.UtcNow.Date` (not full DateTime)

### 2.3: Missing Strategy Tests

- [ ] **ContextAnalysis Strategy**:
  - Test `GenerateContextBasedDefault` method (currently falls back to pattern/type)
  - Test with context parameter (even though it's not fully implemented)

### 2.4: Missing Edge Cases

- [ ] **Null Context**:
  - Test `GenerateDefault` with null context
  - Verify graceful handling

- [ ] **Unknown FieldType**:
  - Test default case in `GenerateTypeBasedDefault` switch
  - Verify returns null

- [ ] **Pattern Fallback**:
  - Test pattern-based default that falls back to type-based

### 2.5: Logging Tests (Optional)

- [ ] Test logging calls (if using Moq for ILogger)
- [ ] Verify debug logs are called appropriately

**Files to Modify**:
- `DataAbstractionAPI.Services.Tests/DefaultGeneratorTests.cs`

**Estimated New Tests**: 15-20 tests

---

## Step 3: TypeConverter Coverage Gaps

**Effort**: 2-3 hours  
**Priority**: High

### 3.1: Missing Conversion Combinations

- [ ] **Date Conversions**:
  - Test Date to String conversion
  - Test String to Date with various formats (already partially covered)
  - Test Date to DateTime conversion (compatibility)

- [ ] **Float to Integer Truncation**:
  - Test negative float truncation
  - Test large float truncation
  - Test zero float truncation

- [ ] **Integer to Float**:
  - Test negative integer conversion
  - Test zero integer conversion
  - Test large integer conversion

### 3.2: Missing Conversion Strategy Tests

- [ ] **Truncate Strategy**:
  - Test Truncate strategy for Float to Integer
  - Test Truncate strategy for unsupported conversions (should throw)
  - Test Truncate strategy error handling

- [ ] **SetNull Strategy Edge Cases**:
  - Test SetNull with non-nullable target type
  - Test SetNull with various conversion failures

- [ ] **FailOnError Strategy**:
  - Test FailOnError with various conversion failures
  - Verify exception details

### 3.3: Missing DateTime Format Tests

- [ ] **Additional DateTime Formats**:
  - Test `yyyy-MM-ddTHH:mm:ss.fffZ` format
  - Test `yyyy-MM-dd HH:mm:ss` format
  - Test `yyyy/MM/dd HH:mm:ss` format
  - Test invalid DateTime formats

### 3.4: Missing Date Format Tests

- [ ] **Additional Date Formats**:
  - Test `MM/dd/yyyy` format
  - Test `dd/MM/yyyy` format
  - Test invalid date formats

### 3.5: Missing Edge Cases

- [ ] **Unsupported Conversions**:
  - Test Array to String (should throw)
  - Test Object to String (should throw)
  - Test DateTime to Integer (should throw)
  - Test Boolean to Integer (should throw)

- [ ] **Exception Handling**:
  - Test exception wrapping in `HandleConversionFailure`
  - Test non-ConversionException exceptions

- [ ] **Empty String Edge Cases**:
  - Test empty string to Float (should throw with Cast)
  - Test whitespace string to Integer (should throw)

### 3.6: Missing Boolean Conversion Variants

- [ ] **Additional Boolean String Variants**:
  - Test case-insensitive variants
  - Test whitespace handling
  - Test unrecognized values (should default to false)

**Files to Modify**:
- `DataAbstractionAPI.Services.Tests/TypeConverterTests.cs`

**Estimated New Tests**: 20-25 tests

---

## Step 4: FilterEvaluator Coverage Gaps

**Effort**: 2-3 hours  
**Priority**: High

### 4.1: Missing Compound Filter Edge Cases

- [ ] **Empty Compound Filters**:
  - Test empty AND filter (should return true)
  - Test empty OR filter (should return false)
  - Test null AND filter list
  - Test null OR filter list

- [ ] **Nested Compound Filters**:
  - Test deeply nested AND/OR combinations
  - Test OR within AND within OR

### 4.2: Missing Operator Edge Cases

- [ ] **Null Value Handling**:
  - Test `eq` operator with null values
  - Test `ne` operator with null values
  - Test `gt/gte/lt/lte` with null values (should handle gracefully)
  - Test `in/nin` with null values
  - Test `contains/startswith/endswith` with null values

- [ ] **Type Coercion Edge Cases**:
  - Test numeric comparison with string numbers
  - Test numeric comparison with invalid strings
  - Test decimal/float comparisons
  - Test long integer comparisons

### 4.3: Missing String Operator Edge Cases

- [ ] **String Contains**:
  - Test with empty string
  - Test with case sensitivity
  - Test with special characters

- [ ] **String StartsWith/EndsWith**:
  - Test with empty string
  - Test with whitespace
  - Test with special characters

### 4.4: Missing Array/Collection Tests

- [ ] **ValueInArray Edge Cases**:
  - Test with empty array
  - Test with single-element array
  - Test with non-array enumerable (List, HashSet)
  - Test with string (should not treat as enumerable)

### 4.5: Missing ConvertToDouble Edge Cases

- [ ] **Numeric Type Conversions**:
  - Test with decimal type
  - Test with float type
  - Test with long type
  - Test with invalid string numbers
  - Test with non-numeric strings

### 4.6: Missing Simple Filter Edge Cases

- [ ] **Multiple Field Mismatches**:
  - Test simple filter where first field matches but second doesn't
  - Test simple filter where all fields match

- [ ] **Null Filter**:
  - Test with null filter dictionary (should handle gracefully)

### 4.7: Missing Malformed Filter Tests

- [ ] **Invalid Operator Filter**:
  - Test operator filter with missing "field" key
  - Test operator filter with missing "operator" key
  - Test operator filter with missing "value" key

- [ ] **Malformed Compound Filter**:
  - Test compound filter that falls back to simple filter

**Files to Modify**:
- `DataAbstractionAPI.Services.Tests/FilterEvaluatorTests.cs`

**Estimated New Tests**: 25-30 tests

---

## Step 5: ValidationService Coverage Gaps

**Effort**: 2-3 hours  
**Priority**: High

### 5.1: Missing Argument Validation Tests

- [ ] **Null Parameter Tests**:
  - Test `Validate` with null record (should throw ArgumentNullException)
  - Test `Validate` with null schema (should throw ArgumentNullException)

### 5.2: Missing Type Validation Tests

- [ ] **Array Type**:
  - Test Array type validation
  - Test non-array value with Array type (should fail)

- [ ] **Object Type**:
  - Test Object type validation (Dictionary)
  - Test non-object value with Object type (should fail)

- [ ] **Date Type**:
  - Test Date type validation
  - Test DateTime value with Date type (should pass - compatibility)
  - Test Date value with DateTime type (should pass - compatibility)

### 5.3: Missing Type Coercion Tests

- [ ] **Additional Coercion Scenarios**:
  - Test String to Boolean coercion ("true", "false", "1", "0")
  - Test String to DateTime coercion
  - Test String to Date coercion
  - Test invalid String to Integer coercion (should fail)
  - Test invalid String to Float coercion (should fail)

### 5.4: Missing Numeric Type Tests

- [ ] **Integer Type Variants**:
  - Test long type with Integer field (should pass)
  - Test short type with Integer field (should pass)
  - Test decimal type with Integer field (should pass - via Float compatibility)

- [ ] **Float Type Variants**:
  - Test float type with Float field (should pass)
  - Test decimal type with Float field (should pass)
  - Test integer type with Float field (should pass - compatibility)

### 5.5: Missing Edge Cases

- [ ] **Empty Values**:
  - Test empty string with required String field (should pass - not null)
  - Test empty array with required Array field
  - Test empty dictionary with required Object field

- [ ] **Type Compatibility Edge Cases**:
  - Test Integer/Float compatibility in both directions
  - Test DateTime/Date compatibility in both directions

### 5.6: Missing GetValueType Tests (Indirect)

- [ ] **Type Detection**:
  - Test GetValueType with various types (long, short, decimal, float)
  - Test GetValueType with IEnumerable (non-string)
  - Test GetValueType with Dictionary

**Files to Modify**:
- `DataAbstractionAPI.Services.Tests/ValidationServiceTests.cs`

**Estimated New Tests**: 20-25 tests

---

## Step 6: Integration Test Coverage

**Effort**: 1-2 hours  
**Priority**: Medium

### 6.1: Additional Integration Scenarios

- [ ] **Service Error Handling**:
  - Test DefaultGenerator with invalid context
  - Test TypeConverter error propagation
  - Test FilterEvaluator error handling in adapter context
  - Test ValidationService error handling in adapter context

- [ ] **Service Combination Edge Cases**:
  - Test DefaultGenerator + TypeConverter with edge cases
  - Test FilterEvaluator + TypeConverter with type mismatches
  - Test all services with null/empty data

**Files to Modify**:
- `DataAbstractionAPI.Services.Tests/ServiceIntegrationTests.cs`

**Estimated New Tests**: 5-10 tests

---

## Step 7: Verify Coverage

**Effort**: 1 hour  
**Priority**: High

### 7.1: Run Final Coverage Analysis

```bash
# Run tests with coverage
dotnet test DataAbstractionAPI.Services.Tests /p:CollectCoverage=true /p:CoverletOutputFormat=opencover /p:CoverletOutput=./coverage/

# Generate report
reportgenerator -reports:./coverage/coverage.opencover.xml -targetdir:./coverage/html -reporttypes:Html
```

### 7.2: Verify Coverage Targets

- [ ] **DefaultGenerator**: >85% coverage
- [ ] **TypeConverter**: >85% coverage
- [ ] **FilterEvaluator**: >85% coverage
- [ ] **ValidationService**: >85% coverage
- [ ] **Overall Services**: >85% coverage

### 7.3: Document Results

- [ ] Update IMPLEMENTATION_PLAN.md with coverage percentages
- [ ] Check off "Test coverage > 85% for services" in Phase 2 checklist
- [ ] Document any remaining gaps (if <85% in specific areas)

**Validation**: All services achieve >85% coverage, documentation updated

---

## Step 8: Optional Enhancements

**Effort**: 1-2 hours  
**Priority**: Low

### 8.1: Coverage Threshold Enforcement

- [ ] Add coverage threshold to test project:
  ```xml
  <PropertyGroup>
    <Threshold>85</Threshold>
    <ThresholdType>line</ThresholdType>
    <ThresholdStat>total</ThresholdStat>
  </PropertyGroup>
  ```

### 8.2: CI/CD Integration

- [ ] Add coverage check to build pipeline
- [ ] Fail build if coverage < 85%
- [ ] Generate coverage badge (optional)

### 8.3: Coverage Reports in Repository

- [ ] Add coverage reports to .gitignore (if not already)
- [ ] Document how to generate coverage reports in README

---

## Summary

### Estimated Total Effort
- **Step 1**: 30 minutes (measurement)
- **Step 2**: 2-3 hours (DefaultGenerator)
- **Step 3**: 2-3 hours (TypeConverter)
- **Step 4**: 2-3 hours (FilterEvaluator)
- **Step 5**: 2-3 hours (ValidationService)
- **Step 6**: 1-2 hours (Integration)
- **Step 7**: 1 hour (verification)
- **Step 8**: 1-2 hours (optional)

**Total**: 12-18 hours

### Estimated New Tests
- DefaultGenerator: 15-20 tests
- TypeConverter: 20-25 tests
- FilterEvaluator: 25-30 tests
- ValidationService: 20-25 tests
- Integration: 5-10 tests

**Total**: 85-110 new tests

### Success Criteria

✅ All services achieve >85% line coverage  
✅ All critical paths are tested  
✅ Edge cases are covered  
✅ Integration scenarios are tested  
✅ Coverage is documented and verified  
✅ IMPLEMENTATION_PLAN.md is updated

---

## Notes

1. **Coverage Tool**: Coverlet is already installed in the test project
2. **Report Generator**: May need to install `dotnet-reportgenerator-globaltool` for HTML reports
3. **Priority**: Focus on Steps 1-7 first, Step 8 is optional
4. **Testing Strategy**: Use TDD approach - write tests first, verify they fail, then ensure they pass
5. **Code Quality**: Maintain existing test patterns and organization

---

## Related Documentation

- **IMPLEMENTATION_PLAN.md**: Phase 2 checklist
- **TEST_IMPLEMENTATION_SUMMARY.md**: Current test summary
- **TEST_GAPS_ANALYSIS.md**: Previous gap analysis

