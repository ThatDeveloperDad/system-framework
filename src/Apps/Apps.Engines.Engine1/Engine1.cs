using Apps.Engines.Engine1.Abstractions;

namespace Apps.Engines.Engine1;

public class CalcEngine : IEngine1
{
    public int CalcSum(int[] numbers)
    {
        int sum = 0;
        foreach (int num in numbers)
        {
            sum += num;
        }
        return sum;
    }
}

