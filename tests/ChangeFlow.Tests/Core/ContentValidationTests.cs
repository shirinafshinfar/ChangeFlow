using ChangeFlow.Core;
using Xunit;
namespace ChangeFlow.Tests.Core;
public sealed class ContentValidationTests
{
    [Theory]
    [InlineData(null,false)]
    [InlineData("",false)]
    [InlineData(" \t\r\n",false)]
    [InlineData("ok",true)]
    public void TitleMustContainContent(string? title,bool expected)=>Assert.Equal(expected,ContentValidation.IsValid(title,null,null));
    [Fact]
    public void ScalarLengthLimitsIncludeSupplementaryAndCombiningCharacters()
    {
        var emoji=char.ConvertFromUtf32(0x1F642);
        Assert.True(ContentValidation.IsValid(string.Concat(Enumerable.Repeat(emoji,200)),new string('d',5000),new string('r',2000)));
        Assert.False(ContentValidation.IsValid(string.Concat(Enumerable.Repeat(emoji,201)),null,null));
        Assert.True(ContentValidation.IsValid(string.Concat(Enumerable.Repeat("a\u0301",100)),null,null));
        Assert.False(ContentValidation.IsValid(string.Concat(Enumerable.Repeat("a\u0301",101)),null,null));
        Assert.False(ContentValidation.IsValid("ok",new string('d',5001),null));
        Assert.False(ContentValidation.IsValid("ok",null,new string('r',2001)));
        Assert.True(ContentValidation.IsValid("ok","",null));
    }
}
