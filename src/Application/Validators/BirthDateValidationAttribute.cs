using System.ComponentModel.DataAnnotations;

namespace TiendaUCN.src.Application.Validators
{
    public class BirthDateValidationAttribute : ValidationAttribute
    {
        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            string valueString = value.ToString()!;

            if (!DateTime.TryParse(valueString, out DateTime date))
            {
                return new ValidationResult("El formato de la Fecha de Nacimiento no es valido.");
            }

            if (date > DateTime.Today)
            {
                return new ValidationResult("La fecha de nacimiento no puede ser futura.");
            }

            if (date < DateTime.Today.AddYears(-120))
            {
                return new ValidationResult("La fecha de nacimiento no puede corresponder a una edad mayor a 120 años.");
            }

            if (date > DateTime.Today.AddYears(-18))
            {
                return new ValidationResult("La fecha de nacimiento debe corresponder a una persona mayor de edad.");
            }

            return ValidationResult.Success;
        }
    }
}
