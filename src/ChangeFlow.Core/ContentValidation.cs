using System.Text;
namespace ChangeFlow.Core;
public static class ContentValidation
{
    public static bool IsValid(string? title,string? description,string? reason)=>
        !string.IsNullOrWhiteSpace(title)&&Count(title)<=200&&Count(description)<=5000&&Count(reason)<=2000;
    private static int Count(string? text) {var count=0;if(text is not null)foreach(var rune in text.EnumerateRunes())count++;return count;}
}
