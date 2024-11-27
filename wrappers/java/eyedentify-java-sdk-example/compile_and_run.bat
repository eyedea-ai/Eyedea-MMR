@rem COMMANDLINE EXAMPLE COMPILATION AND RUN
mkdir classes
javac -d classes\ -cp ..\eyedentify-java-sdk.jar src\cz\eyedea\eyedentify\java\example\ExampleMMR.java
java -cp ..\eyedentify-java-sdk.jar;classes cz.eyedea.eyedentify.java.example.ExampleMMR
