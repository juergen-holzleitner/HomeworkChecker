import java.util.Scanner;

public class DigitSum {
    public static void main(String[] args){
        Scanner scanner = new Scanner(System.in);

        System.out.print("Number: ");
        int number = scanner.nextInt();

        if (number >= 0){
            int sum = calculateDigitSum(number);
            System.out.print("DigitSum: " + sum);
        } else {
            System.out.print("Invalid number");
        }
    }
    private static int caculateDigitSum(int number){
        int sum = 0;
        int decimalPointNumber;
        while (number > 0) {
            decimalPointNumber = number % 10;
            sum += decimalPointNumber;
            number /= 10;
        }
        return sum;
    }
}
