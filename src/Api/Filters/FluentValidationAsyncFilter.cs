using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Globalization;
using System.Reflection;

namespace Api.Filters
{
    public class FluentValidationAsyncFilter : IAsyncActionFilter
    {
        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var ct = context.HttpContext.RequestAborted;
            var failures = new List<ValidationFailure>();

            foreach (var (_, argValue) in context.ActionArguments)
            {
                if (argValue is null) continue;

                // ✅ injeta parâmetros de rota (id, empresaId, etc.) no DTO ANTES de validar
                InjectRouteValuesIntoDto(context, argValue);

                var argType = argValue.GetType();
                var validatorType = typeof(IValidator<>).MakeGenericType(argType);

                // Suporta 0..N validators para o mesmo tipo
                var validators = context.HttpContext.RequestServices.GetServices(validatorType)
                    .OfType<IValidator>()
                    .ToList();

                if (validators.Count == 0)
                    continue;

                var fvContext = new ValidationContext<object>(argValue);

                foreach (var validator in validators)
                {
                    var result = await validator.ValidateAsync(fvContext, ct);
                    if (!result.IsValid)
                        failures.AddRange(result.Errors);
                }
            }

            if (failures.Count > 0)
            {
                throw new ValidationException(failures);
            }

            await next();
        }
        private static void InjectRouteValuesIntoDto(ActionExecutingContext context, object dto)
        {
            var routeValues = context.RouteData.Values;
            if (routeValues is null || routeValues.Count == 0) return;

            var dtoType = dto.GetType();

            // pega props setáveis
            var props = dtoType
                .GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(p => p.CanWrite)
                .ToList();

            // índice case-insensitive
            var propByName = props.ToDictionary(p => p.Name, StringComparer.OrdinalIgnoreCase);

            foreach (var kvp in routeValues)
            {
                var key = kvp.Key;                // ex: "id", "empresaId"
                var raw = kvp.Value?.ToString();  // ex: "10"
                if (string.IsNullOrWhiteSpace(raw)) continue;

                // 1) tenta bater direto pelo nome (empresaId -> EmpresaId)
                if (propByName.TryGetValue(key, out var directProp))
                {
                    TrySet(directProp, dto, raw);
                    continue;
                }

                // 2) alias clássico: rota "id" -> propriedade "Id" (ou <Algo>Id)
                if (key.Equals("id", StringComparison.OrdinalIgnoreCase))
                {
                    if (propByName.TryGetValue("Id", out var idProp))
                    {
                        TrySet(idProp, dto, raw);
                        continue;
                    }

                    // fallback: se não tem "Id", tenta alguma propriedade que termine com "Id"
                    // (ex.: CadeiraId, EmpresaId etc.)
                    var anyIdProp = props.FirstOrDefault(p =>
                        p.Name.EndsWith("Id", StringComparison.OrdinalIgnoreCase) &&
                        (Nullable.GetUnderlyingType(p.PropertyType) ?? p.PropertyType) == typeof(int));

                    if (anyIdProp is not null)
                        TrySet(anyIdProp, dto, raw);
                }
            }
        }

        private static bool TrySet(PropertyInfo prop, object target, string raw)
        {
            var targetType = Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType;

            try
            {
                var converted = ConvertTo(targetType, raw);
                if (converted is null) return false;

                prop.SetValue(target, converted);
                return true;
            }
            catch
            {
                // se não conseguir converter, ignora (validator pode acusar)
                return false;
            }
        }

        private static object? ConvertTo(Type type, string raw)
        {
            if (type == typeof(string)) return raw;

            if (type == typeof(int)) return int.Parse(raw, CultureInfo.InvariantCulture);
            if (type == typeof(long)) return long.Parse(raw, CultureInfo.InvariantCulture);
            if (type == typeof(short)) return short.Parse(raw, CultureInfo.InvariantCulture);

            if (type == typeof(bool))
            {
                if (raw == "1") return true;
                if (raw == "0") return false;
                return bool.Parse(raw);
            }

            if (type == typeof(Guid)) return Guid.Parse(raw);

            if (type == typeof(DateTime))
                return DateTime.Parse(raw, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind);

            if (type.IsEnum)
                return Enum.Parse(type, raw, ignoreCase: true);

            return Convert.ChangeType(raw, type, CultureInfo.InvariantCulture);
        }
    }
}
