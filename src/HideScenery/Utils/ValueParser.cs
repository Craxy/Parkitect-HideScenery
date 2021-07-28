using System;

namespace Craxy.Parkitect.HideScenery.Utils
{
  public sealed class ValueParser<TResult>
    where TResult : struct
  {
    private string _input;
    private TResult? _lastParseResult;

    public ValueParser(Func<string, TResult?> parse, string initialInput)
    {
      Parse = parse;

      Input = initialInput;

      if (!LastParseResult.HasValue)
      {
        throw new ArgumentException("Initial Input must parse to a valid value.", nameof(initialInput));
      }
    }

    public readonly Func<string, TResult?> Parse;
    public TResult Value { get; private set; }

    public string Input
    {
      get => _input;
      set
      {
        if (value != _input)
        {
          _input = value;
          LastParseResult = Parse(value);
        }
      }
    }

    public TResult? LastParseResult
    {
      get => _lastParseResult;
      private set
      {
        if (!value.Equals(_lastParseResult))
        {
          _lastParseResult = value;

          if (value.HasValue)
          {
            Value = value.Value;
          }
        }
      }
    }

    public bool IsValidInput => LastParseResult.HasValue;
  }

  public static class Parser
  {
    public static float? Float(string value)
    {
      if (float.TryParse(value, out var v))
      {
        return v;
      }
      else
      {
        return null;
      }
    }
    public static int? Int(string value)
    {
      if (int.TryParse(value, out var v))
      {
        return v;
      }
      else
      {
        return null;
      }
    }
  }
}