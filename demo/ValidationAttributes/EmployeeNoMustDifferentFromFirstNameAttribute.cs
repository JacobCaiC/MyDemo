using MyDemo.Models;
using MyDemo.Models.Dto;
using System.ComponentModel.DataAnnotations;

namespace MyDemo.ValidationAttributes
{
    public class EmployeeNoMustDifferentFromFirstNameAttribute : ValidationAttribute
    {
        //当作用于属性时 value 是属性值，作用于类时 value 是对象
        //validationContext 是属性所在的对象
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            //var employeeAddDto = (EmployeeAddDto)value
            var employeeAddDto = (EmployeeAddOrUpdateDto)validationContext.ObjectInstance;
            if (employeeAddDto.EmployeeNo == employeeAddDto.FirstName)
            {
                return new ValidationResult(ErrorMessage, new[] { nameof(EmployeeAddOrUpdateDto) });
            }
            return ValidationResult.Success;
        }
    }
}
