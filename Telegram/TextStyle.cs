namespace InstaFollowersOverseer;

[Flags]
public enum TextStyle
{
    PlainText=0,
    Bold=1, Italic=2, Crossed=4, Underline=8,
    Spoiler=16, Link=32, CodeLine=64, CodeBlock=128
}