
using System.Net.Mime;
using Microsoft.Extensions.Primitives;

namespace InstaFollowersOverseer;

/// <summary>
/// Class that builds selegram message with html tags.
/// It is a state machine.
/// Exapmple:
///  SetUrl("https://x.com").BeginStyle(TextStyle.Url).Text("X").EndStyle()
///  opens html a tag with href=https://x.com and content X, then closes tag
/// </summary>
// supported tags:
// <b>bold</b>
// <i>italic</i> 
// <s>crossed</s>
// <u>underline</u>
// <tg-spoiler>spoiler</tg-spoiler>
// <a href="http://www.example.com/">inline URL</a>
// <a href="tg://user?id=123456789">inline mention of a user</a>
// <code>inlne code</code>
// <pre language="c++">code block</pre>
public class HtmlMessageBuilder
{
    record struct BuilderState(TextStyle Style, string? Url = null, long? UserId = null, string? CodeLang = null)
    {
        public void Reset()
        {
            Style = TextStyle.PlainText;
            Url = null;
            UserId = null;
            CodeLang = null;
        }
    }

    private BuilderState _state=new(TextStyle.PlainText);
    StringBuilder _plainText=new();
    StringBuilder _html=new();

    protected void ReplaceHtmlReservedChar(char c) =>
        _html.Append(c switch
        {
            '<'=>"&lt",
            '>'=>"&gt",
            '&'=>"&apm",
            '"'=>"&quot",
            '\''=>"&apos",
            _ => c
        });
    
    protected void ReplaceHtmlReservedChars(ReadOnlySpan<char> text)
    {
        for (int i = 0; i < text.Length; i++) 
            ReplaceHtmlReservedChar(text[i]);
    }

    /// opens html tags enabled in state fields
    protected void OpenTags()
    {
        if(_state.Style==TextStyle.PlainText)
            return;
        
        // the order of fields is very importang, it must be in reversed in CloseTags()
        if (0!=(_state.Style & TextStyle.Bold))  _html.Append("<b>");
        if (0!=(_state.Style & TextStyle.Italic)) _html.Append("<i>");
        if (0!=(_state.Style & TextStyle.Crossed))  _html.Append("<s>");
        if (0!=(_state.Style & TextStyle.Underline)) _html.Append("<u>");
        if (0!=(_state.Style & TextStyle.Spoiler)) _html.Append("<tg-spoiler>");
        if (0!=(_state.Style & TextStyle.Link))
        {
            _html.Append("<a href='");
            if (_state.UserId is not null)
                _html.Append("tg://user?id=").Append(_state.UserId);
            else if (!_state.Url.IsNullOrEmpty()) 
                _html.Append(_state.Url);
            else throw new Exception("empty url");
            _html.Append("'>");
        }
        if (0!=(_state.Style & TextStyle.CodeLine)) _html.Append("<code>");
        if (0!=(_state.Style & TextStyle.CodeBlock)) 
        { 
            _html.Append("<pre");
            if (!_state.CodeLang.IsNullOrEmpty())
                _html.Append(" language='").Append(_state.CodeLang).Append('\'');
            _html.Append('>');
        }
    }

    /// closes opened html tags 
    protected void CloseTags()
    {
        if(_state.Style==TextStyle.PlainText)
            return;
        
        // the order of fields is very importang, it must be in reversed in CloseTags()
        if (0!=(_state.Style & TextStyle.CodeBlock)) _html.Append("</pre>");
        if (0!=(_state.Style & TextStyle.CodeLine))  _html.Append("</code>");
        if (0!=(_state.Style & TextStyle.Link))      _html.Append("</a>");
        if (0!=(_state.Style & TextStyle.Spoiler))   _html.Append("</tg-spoiler>");
        if (0!=(_state.Style & TextStyle.Underline)) _html.Append("</u>");
        if (0!=(_state.Style & TextStyle.Crossed))   _html.Append("</s>");
        if (0!=(_state.Style & TextStyle.Italic))    _html.Append("</i>");
        if (0!=(_state.Style & TextStyle.Bold))      _html.Append("</b>");
        _state.Reset();
    }

    /// appends text to builder
    public HtmlMessageBuilder Text(string text)
    {
        _plainText.Append(text);
        ReplaceHtmlReservedChars(text);
        return this;
    }
    public HtmlMessageBuilder Text(char ch)
    {
        _plainText.Append(ch);
        ReplaceHtmlReservedChar(ch);
        return this;
    }
    public HtmlMessageBuilder Text(int o)
    {
        string text = o.ToString();
        _plainText.Append(text);
        ReplaceHtmlReservedChars(text);
        return this;
    }
    public HtmlMessageBuilder Text(long o)
    {
        string text = o.ToString();
        _plainText.Append(text);
        ReplaceHtmlReservedChars(text);
        return this;
    }
    public HtmlMessageBuilder Text(object o)
    {
        if (o is null)
            throw new NullReferenceException("object is null");
        string text = o.ToString()!;
        _plainText.Append(text);
        ReplaceHtmlReservedChars(text);
        return this;
    }
    
    /// enables specified styles
    public HtmlMessageBuilder BeginStyle(TextStyle style)
    {
        if (_state.Style != TextStyle.PlainText)
            throw new Exception("can't begin new style before ending previous");
        _state.Style = style;
        OpenTags();
        return this;
    }

    /// removes all styles
    public HtmlMessageBuilder EndStyle()
    {
        CloseTags();
        return this;
    }
    
    // use before BeginStyle
    public HtmlMessageBuilder SetUrl(string url) { _state.Url = url; return this; }
    public HtmlMessageBuilder SetUserMention(long id) { _state.UserId=id ; return this; }
    public HtmlMessageBuilder SetCodeLanguage(string codeLang="") { _state.CodeLang=codeLang ; return this; }
    
    public string ToPlainText() => _plainText.ToString();
    public string ToHtml() => _html.ToString();
    
    #if DEBUG
    public override string ToString() => ToHtml();
    #else
    public override string ToString() => ToPlainText();
    #endif

    public void Clear()
    {
        _plainText.Clear();
        _html.Clear();
        _state.Reset();
    }
}