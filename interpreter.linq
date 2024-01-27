<Query Kind="Program" />

void Main()
{
    Env emptyEnv = x => null;

    LambdaExpr multiplyExpr = new LambdaExpr("x",
        new LambdaExpr("y", new Func<object, object, object>((x, y) => (int)x * (int)y)));

    LambdaExpr squareExpr = new LambdaExpr("n",
        new ApplicationExpr(
            new ApplicationExpr(multiplyExpr, "n"),
            "n"));

    Lambda squareFn = (Lambda)Interpreter.Eval(squareExpr, emptyEnv);

    squareFn(5).Dump();
}

public delegate object Env(string symbol);
public delegate object Lambda(object arg);

public class Interpreter
{
    public static object Eval(object expr, Env env)
    {
        switch (expr)
        {
            case string x:
                return env(x);

            case LambdaExpr lambdaExpr:
                return new Lambda(arg =>
                {
                    Env extendedEnv = y =>
                    {
                        if (y == lambdaExpr.Parameter)
                            return arg;
                        else
                            return env(y);
                    };
                    return Eval(lambdaExpr.Body, extendedEnv);
                });

            case ApplicationExpr appExpr:
                var rator = Eval(appExpr.Operator, env);
                if (rator is Lambda lambda)
                {
                    var rand = Eval(appExpr.Operand, env);
                    return lambda(rand);
                }
                else
                {
                    throw new InvalidOperationException("operator must be a lambda");
                }

            case Func<object, object, object> func:
                return new Lambda(arg1 => new Lambda(arg2 => func(arg1, arg2)));

            default:
                throw new ArgumentException($"unhandled expr: {expr.GetType()}");
        }
    }
}

public class LambdaExpr
{
    public string Parameter { get; }
    public object Body { get; }

    public LambdaExpr(string parameter, object body)
    {
        Parameter = parameter;
        Body = body;
    }
}

public class ApplicationExpr
{
    public object Operator { get; }
    public object Operand { get; }

    public ApplicationExpr(object operatorExpr, object operand)
    {
        Operator = operatorExpr;
        Operand = operand;
	}
}