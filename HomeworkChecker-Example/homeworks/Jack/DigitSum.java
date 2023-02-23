import java.util.Scanner;
public class DigitSum{
    public static void main(String[] args){
        Scanner scanner = new Scanner(System.in);
        int number = readParameter(scanner, "Number: ");
        if(number > 0){
            System.out.print("DigitSum:" + sumOfDigits(number));
        }else{
            System.out.print("Invalid Number");
        }
    }
    private static int readParameter(Scanner scanner, String message){
        System.out.print(message);
        int input = scanner.nextInt();
        return(input);
        }
    private static int sumOfDigits(int number){
        int digitSum = 0;
        while(number > 0){
            digitSum = digitSum + number % 10;
            number = number /= 10;
        }
        return digitSum;
    }
}
