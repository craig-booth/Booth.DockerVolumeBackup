using ErrorOr;

namespace Booth.DockerVolumeBackup.WebApi.Extensions
{
    public static class ErrorResult
    {

        public static IResult Error(List<Error> errors)
        {
            if (errors.Count == 0)
            {
                return TypedResults.Problem();
            }

            if (errors.All(error => error.Type == ErrorType.Validation))
            {
                var validationErrors = new Dictionary<string, string[]>();
                foreach (var e in errors)
                {
                    if (validationErrors.Remove(e.Code, out var value))
                        validationErrors.Add(e.Code, [.. value, e.Description]);
                    else
                        validationErrors.Add(e.Code, [e.Description]);
                }

                return TypedResults.ValidationProblem(validationErrors, title: "One or more validation errors occurred.");
            }

            var statusCode = errors[0].Type switch
            {
                ErrorType.Conflict => StatusCodes.Status409Conflict,
                ErrorType.Validation => StatusCodes.Status400BadRequest,
                ErrorType.NotFound => StatusCodes.Status404NotFound,
                _ => StatusCodes.Status500InternalServerError
            };

            return TypedResults.Problem(statusCode: statusCode, title: errors[0].Description);

        }
    }
}
