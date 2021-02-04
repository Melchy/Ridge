using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Ridge.Results;
using System;

namespace Ridge.Setup
{
    public class ControllerResultTypeMapper : IActionResultTypeMapper
    {
        public virtual Type GetResultDataType(Type returnType)
        {
            if (returnType == null)
            {
                throw new ArgumentNullException(nameof(returnType));
            }

            if (returnType == typeof(ControllerResult))
            {
                return null!;
            }

            if(returnType.GetGenericTypeDefinition() == typeof(ControllerResult<>))
            {
                return returnType.GetGenericArguments()[0];
            }

            // Copy of Microsoft.AspNetCore.Mvc.Infrastructure.ActionResultTypeMapper
            if (returnType.IsGenericType &&
                returnType.GetGenericTypeDefinition() == typeof(ActionResult<>))
            {
                return returnType.GetGenericArguments()[0];
            }

            return returnType;
        }

        // Copy of Microsoft.AspNetCore.Mvc.Infrastructure.ActionResultTypeMapper
        public virtual IActionResult Convert(object value, Type returnType)
        {
            if (returnType == null)
            {
                throw new ArgumentNullException(nameof(returnType));
            }

            if (value is IConvertToActionResult converter)
            {
                return converter.Convert();
            }

            return new ObjectResult(value)
            {
                DeclaredType = returnType,
            };
        }
    }
}
