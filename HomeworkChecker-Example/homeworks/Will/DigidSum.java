import java.util.Scanner;

public class DigidSum {
    public static void main(String[] args){
        Scanner scanner = new Scanner(System.in);

        
        System.out.print("Number: ");
        int number = scanner.nextInt();

        if(number >= 0){
           int sum = findDigitSum(number);
            System.out.print("DigitSum: " + sum);
        }else{
            System.out.print("Invalid number");
        }
    }
    
    private static int findDigitSum(int number){
        int sum = 0;
        int commaNumber;
        while(number > 0){
            commaNumber = number % 10;
            sum += commaNumber;
            number /= 10;
        }
        return sum;
    }
}
