using System.Runtime.ExceptionServices;

int steps = 0;
int[] numbers = { 1, 2, 3, 5, 4};
int count1 = 0;
int swapCount = 0;
bool swap = true;
while (count1 < numbers.Length - 1 && swap)
{
    for (int count2 = 0;count2 < numbers.Length - 1 - count1; count2++)
    {
        steps++;
        if (numbers[count2] > numbers[count2 + 1])
        {
            int temp = numbers[count2];
            numbers[count2] = numbers[count2 + 1];
            numbers[count2 + 1] = temp;
            swapCount++;
        }
        if (swapCount == 1 || swapCount == 0)
        {
            swap = false;
        }   
 
    }
    count1++;
}
Console.WriteLine(steps);