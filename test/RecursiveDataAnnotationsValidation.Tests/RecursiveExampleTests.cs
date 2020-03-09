using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using RecursiveDataAnnotationsValidation.Tests.TestModels;
using Xunit;

namespace RecursiveDataAnnotationsValidation.Tests
{
    public class RecursiveExampleTests
    {
        private readonly IRecursiveDataAnnotationValidator _validator = new RecursiveDataAnnotationValidator();
        
        // Verify that we can recursively validate, but avoid infinite loops
        
        [Fact]
        public void Passes_all_validation()
        {
            var recursion = new RecursiveExample
            {
                Name = "Recursion1",
                BooleanA = false,
                Recursive = new RecursiveExample
                {
                    Name = "Recursion1.Inner1",
                    BooleanA = true,
                    Recursive = null
                }
            };
            recursion.Recursive.Recursive = recursion;
            
            var sut = new RecursiveExample
            {
                Name = "SUT",
                BooleanA = true,
                Recursive = recursion
            };
            
            var validationResults = new List<ValidationResult>();
            var result = _validator.TryValidateObjectRecursive(sut, validationResults);
            
            Assert.True(result);
            Assert.Empty(validationResults);
        }        
        
        [Fact]
        public void Fails_because_inner1_boolean_is_null()
        {
            var recursion = new RecursiveExample
            {
                Name = "Recursion1",
                BooleanA = false,
                Recursive = new RecursiveExample
                {
                    Name = "Recursion1.Inner1",
                    BooleanA = null,
                    Recursive = null
                }
            };
            recursion.Recursive.Recursive = recursion;
            
            var sut = new RecursiveExample
            {
                Name = "SUT",
                BooleanA = true,
                Recursive = recursion
            };
            
            var validationResults = new List<ValidationResult>();
            var result = _validator.TryValidateObjectRecursive(sut, validationResults);
            
            Assert.False(result);
            Assert.NotEmpty(validationResults);
            Assert.NotNull(validationResults
                .FirstOrDefault(x => x.MemberNames.Contains("Recursive.Recursive.BooleanA")));
        }        
    }
}