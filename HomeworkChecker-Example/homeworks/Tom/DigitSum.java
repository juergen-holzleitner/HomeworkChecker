public class DigitSum {
    public static void main (String[] args) {
        java.util.Scanner scanner = new java.util.Scanner(System.in);

        System.out.print("Number: ");
        int number = scanner.nextInt();

        if (number >= 0) {
           int sum = DigitSum(number);
            System.out.print("DigitSum: " + sum);
        } else {
            System.out.print("Invalid number");
        }
    }
    private static int DigitSum(int number) {
        int sum = 0;
        int commaNumber;
        while(number > 0) {
            commaNumber = number % 10;
            sum += commaNumber;
            number /= 10;
        }
        return sum;
    }
}
