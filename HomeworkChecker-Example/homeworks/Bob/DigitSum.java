public class DigitSum {
    public static void main(String[] args) {
        java.util.Scanner scanner = new java.util.Scanner(System.in);

        System.out.print("Number: ");
        int number = scanner.nextInt();

        if (number >= 0) {
            int sum = digitSum(number);
            System.out.print("DigitSum: " + sum);
        } else {
            System.out.print("Invalid number");
        }
    }

    private static int digitSum(int number){
        int sum = 0;
        int sumDigitsum;

        while (number > 0) {
            sumDigitsum = number % 10;
            sum += sumDigitsum;
            number /= 10;
        }
        return sum;
    }
}
