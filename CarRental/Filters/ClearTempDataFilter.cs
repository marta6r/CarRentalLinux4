using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

public class ClearTempDataFilter : IActionFilter
{
    public void OnActionExecuting(ActionExecutingContext context)
    {
        // Очищаем перед выполнением действия
        var controller = context.Controller as Controller;
        if (controller != null)
        {
            controller.TempData.Remove("Success");
            controller.TempData.Remove("Error");
        }
    }

    public void OnActionExecuted(ActionExecutedContext context)
    {
        // Очищаем после выполнения действия
        var controller = context.Controller as Controller;
        if (controller != null)
        {
            controller.TempData.Remove("Success");
            controller.TempData.Remove("Error");
        }
    }
}