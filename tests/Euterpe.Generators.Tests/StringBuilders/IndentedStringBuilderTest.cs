using Euterpe.Generators.StringBuilders;

namespace Euterpe.Generators.Tests.StringBuilders;

[TestSubject(typeof(IndentedStringBuilder))]
public sealed class IndentedStringBuilderTest
{
    [Test]
    public async Task Append_String_NoIndent_WritesValue()
    {
        var sb = new IndentedStringBuilder();
        sb.Append("hello");
        await Assert.That(sb.ToString()).IsEqualTo("hello");
    }

    [Test]
    public async Task Append_FormattableString_NoIndent_WritesValue()
    {
        var sb = new IndentedStringBuilder();
        FormattableString fs = $"value={42}";
        sb.Append(fs);
        await Assert.That(sb.ToString()).IsEqualTo("value=42");
    }

    [Test]
    public async Task Append_Char_NoIndent_WritesValue()
    {
        var sb = new IndentedStringBuilder();
        sb.Append('x');
        await Assert.That(sb.ToString()).IsEqualTo("x");
    }

    [Test]
    public async Task Append_StringEnumerable_ConcatsValues()
    {
        var sb = new IndentedStringBuilder();
        sb.Append(new[] { "a", "b", "c" });
        await Assert.That(sb.ToString()).IsEqualTo("abc");
    }

    [Test]
    public async Task Append_ReadOnlySpan_AppendsCharByChar()
    {
        var sb = new IndentedStringBuilder();
        sb.Append("span".AsSpan());
        await Assert.That(sb.ToString()).IsEqualTo("span");
    }

    [Test]
    public async Task AppendLine_WithoutValue_AddsLineBreak()
    {
        var sb = new IndentedStringBuilder();
        sb.AppendLine();
        await Assert.That(sb.ToString()).IsEqualTo(Environment.NewLine);
    }

    [Test]
    public async Task AppendLine_String_AppendsValueAndNewLine()
    {
        var sb = new IndentedStringBuilder();
        sb.AppendLine("line");
        await Assert.That(sb.ToString()).IsEqualTo("line" + Environment.NewLine);
    }

    [Test]
    public async Task AppendLine_FormattableString_AppendsValueAndNewLine()
    {
        var sb = new IndentedStringBuilder();
        FormattableString fs = $"x={1}";
        sb.AppendLine(fs);
        await Assert.That(sb.ToString()).IsEqualTo("x=1" + Environment.NewLine);
    }

    [Test]
    public async Task IncreaseIndent_AppliesFourSpacesPerLevel()
    {
        var sb = new IndentedStringBuilder();
        sb.IncreaseIndent().Append("one");
        await Assert.That(sb.ToString()).IsEqualTo("    one");
    }

    [Test]
    public async Task IncreaseIndent_WithCount_StacksLevels()
    {
        var sb = new IndentedStringBuilder();
        sb.IncreaseIndent(2).Append("two");
        await Assert.That(sb.ToString()).IsEqualTo("        two");
    }

    [Test]
    public async Task DecreaseIndent_RemovesOneLevel()
    {
        var sb = new IndentedStringBuilder();
        sb.IncreaseIndent(2).DecreaseIndent().Append("one");
        await Assert.That(sb.ToString()).IsEqualTo("    one");
    }

    [Test]
    public async Task DecreaseIndent_BelowZero_ClampsAtZero()
    {
        var sb = new IndentedStringBuilder();
        sb.DecreaseIndent().DecreaseIndent().Append("x");
        await Assert.That(sb.ToString()).IsEqualTo("x");
    }

    [Test]
    public async Task ResetIndent_RestoresZeroLevel()
    {
        var sb = new IndentedStringBuilder();
        sb.IncreaseIndent(3);
        sb.ResetIndent();
        sb.Append("flush");
        await Assert.That(sb.ToString()).IsEqualTo("flush");
    }

    [Test]
    public async Task AppendLine_AfterIndent_DoesNotIndentBareNewLine()
    {
        var sb = new IndentedStringBuilder();
        sb.IncreaseIndent();
        sb.AppendLine("line").AppendLine();
        await Assert.That(sb.ToString()).IsEqualTo("    line" + Environment.NewLine + Environment.NewLine);
    }

    [Test]
    public async Task FluentChain_ReturnsSameInstance()
    {
        var sb = new IndentedStringBuilder();
        FormattableString fs = $"={1}";
        var r = sb.Append("a")
            .AppendLine()
            .AppendLine("b")
            .Append('c')
            .Append(fs)
            .Append(new[] { "x" })
            .Append("y".AsSpan());

        await Assert.That(r).IsSameReferenceAs(sb);
    }
}