using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace EstateHub.SharedKernel.CustomAttributes;

public class MaxFileSizeAttribute : ValidationAttribute
{
    private readonly int _maxFileSize;

    public MaxFileSizeAttribute(int maxFileSize)
    {
        _maxFileSize = maxFileSize;
    }

    protected override ValidationResult IsValid(object value, ValidationContext validationContext)
    {
        if (value is IFormFile file)
        {
            if (file.Length > _maxFileSize)
            {
                return new ValidationResult($"File size cannot exceed {_maxFileSize / (1024 * 1024)} MB");
            }
        }

        return ValidationResult.Success;
    }
}
