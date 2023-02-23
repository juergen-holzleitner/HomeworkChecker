import java.util.Scanner;

public class DigitSum {
    public static void main(String[] args) {
        final Scanner scanner = new Scanner(System.in);
        
        System.out.print("Number: ");
        final int number = scanner.nextInt();
        if (number < 0) {
            System.out.println("Invalid number");
            return;
        }

        final int digitSum = calculateDigitSum(number);

        System.out.println("DigitSum: " + digitSum);
    }

    private static int calculateDigitSum(int number) {
        int digitSum = 0;
        int currentNumber = number;
        while (currentNumber != 0) {
            digitSum += currentNumber % 10;
            currentNumber /= 10;
        }
        return digitSum;
    }
}