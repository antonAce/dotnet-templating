using System;

namespace Mutant.Chicken.Expressions.Cpp
{
    internal class Scope
    {
        public enum NextPlacement
        {
            Left,
            Right,
            None
        }

        public object Left { get; set; }

        public Operator Operator { get; set; }

        public object Right { get; set; }

        public object Value
        {
            set
            {
                switch (TargetPlacement)
                {
                    case NextPlacement.Left:
                        Left = value;
                        TargetPlacement = NextPlacement.Right;
                        break;
                    case NextPlacement.Right:
                        Right = value;
                        TargetPlacement = NextPlacement.None;
                        break;
                }
            }
        }

        public NextPlacement TargetPlacement { get; set; }

        private static T EvaluateSide<T>(object side, Func<object, T> convert)
        {
            Scope scope = side as Scope;

            if (scope != null)
            {
                return convert(scope.Evaluate());
            }

            return convert(side);
        }

        public static TResult EvaluateSides<TLeft, TRight, TResult>(object left, object right, Func<object, TLeft> convertLeft, Func<object, TRight> convertRight, Func<TLeft, TRight, TResult> combine)
        {
            TLeft l = EvaluateSide(left, convertLeft);
            TRight r = EvaluateSide(right, convertRight);
            return combine(l, r);
        }

        public static TResult EvaluateSides<T, TResult>(object left, object right, Func<object, T> convert, Func<T, T, TResult> combine)
        {
            return EvaluateSides(left, right, convert, convert, combine);
        }

        public object Evaluate()
        {
            switch (Operator)
            {
                case Operator.Not:
                    return !EvaluateSide(Right, x => (bool) x);
                case Operator.And:
                    return EvaluateSides(Left, Right, x => (bool) x, (x, y) => x && y);
                case Operator.Or:
                    return EvaluateSides(Left, Right, x => (bool)x, (x, y) => x || y);
                case Operator.Xor:
                    return EvaluateSides(Left, Right, x => (bool)x, (x, y) => x ^ y);
                case Operator.EqualTo:
                    return EvaluateSides(Left, Right, x => x, Equals);
                case Operator.NotEqualTo:
                    return EvaluateSides(Left, Right, x => x, (x, y) => !Equals(x, y));
                case Operator.GreaterThan:
                    return EvaluateSides(Left, Right, Convert.ToDouble, (x, y) => x > y);
                case Operator.GreaterThanOrEqualTo:
                    return EvaluateSides(Left, Right, Convert.ToDouble, (x, y) => x >= y);
                case Operator.LessThan:
                    return EvaluateSides(Left, Right, Convert.ToDouble, (x, y) => x < y);
                case Operator.LessThanOrEqualTo:
                    return EvaluateSides(Left, Right, Convert.ToDouble, (x, y) => x <= y);
                case Operator.LeftShift:
                    return EvaluateSides(Left, Right, Convert.ToInt64, Convert.ToInt32, (x, y) => x << y);
                case Operator.RightShift:
                    return EvaluateSides(Left, Right, Convert.ToInt64, Convert.ToInt32, (x, y) => x >> y);
                case Operator.BitwiseAnd:
                    return EvaluateSides(Left, Right, Convert.ToInt64, (x, y) => x & y);
                case Operator.BitwiseOr:
                    return EvaluateSides(Left, Right, Convert.ToInt64, (x, y) => x | y);
                default:
                    if (Left != null)
                    {
                        return EvaluateSide(Left, x => (bool) x);
                    }

                    return false;
            }
        }
    }
}