import java.util.Scanner;

public class DigitSum {
    public static void main(String[] args) {
        Scanner scanner = new Scanner(System.in);

        int numberForDigitSum = readParameter(scanner, "Number: ");

        if (numberForDigitSum < 0) {
            System.out.print("Invalid number");
            return;
        }

        int sum = digitSumCalculation(numberForDigitSum);

        System.out.format("DigitSum: %d%n", sum);
    }

    private static int readParameter(Scanner scanner, String parameterName) {
        System.out.print(parameterName);
        return scanner.nextInt();
    }

    private static int digitSumCalculation(int numberForDigitSum) {
        int digitSum = 0;
        int digit = 0;
        do {
            digit = numberForDigitSum % 10;
            numberForDigitSum = numberForDigitSum / 10;
            digitSum = digitSum + digit;
        } while (numberForDigitSum > 0);

        return digitSum;
    }
}
