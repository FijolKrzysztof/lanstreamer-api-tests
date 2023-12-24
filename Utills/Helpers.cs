using Microsoft.AspNetCore.Mvc;

namespace lanstreamer_api_tests.Utills;

public class Helpers
{
    public static T GetActionResultContent<T>(ActionResult<T> result)
    {
        return (T) ((ObjectResult) result.Result).Value;
    }
}