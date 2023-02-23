import java.util.Scanner;
public class DigitSum {
    public static void main(String[] args) {
        Scanner scanner = new Scanner(System.in);    
        int digitSum = 0;
        do{
            System.out.print("Number: ");
            digitSum = scanner.nextInt();
            if(digitSum <= 0){ 
                System.out.println("Invalid Number"); 
            }
        
       }while(digitSum <= 0);
       System.out.println("DigitSum: " + getDigitSum(digitSum));
       scanner.close();
    } 
    private static int getDigitSum(int digitSum){
        int summary = 0;
        int digit; 
        while (digitSum > 0 ) {
            digit = digitSum% 10;                         
            summary = summary + digit; 
            digitSum = digitSum / 10; 
        }       
        return summary;
    }
}
