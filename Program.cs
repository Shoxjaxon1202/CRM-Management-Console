using System;
using System.Collections.Generic;
using System.Linq;
using Spectre.Console;

abstract class Person
{
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string Email { get; set; }

    public abstract void DisplayInfo();
}

interface ISalable
{
    void AddSale(decimal saleAmount);
    List<decimal> GetSales();
}

interface IPayment
{
    decimal GetTotalPayments();
    void AddPayment(decimal payment);
}

interface ISearchable
{
    void Search(string query);
}

class Customer : Person, ISalable, IPayment
{
    public List<decimal> Sales { get; set; } = new List<decimal>();

    public Customer(string firstName, string lastName, string email)
    {
        FirstName = firstName;
        LastName = lastName;
        Email = email;
    }

    public override void DisplayInfo()
    {
        var table = new Table();
        table.AddColumn("First Name");
        table.AddColumn("Last Name");
        table.AddColumn("Email");
        table.AddColumn("Total Sales");
        table.AddColumn("Total Payments");

        table.AddRow(FirstName, LastName, Email, Sales.Count.ToString(), GetTotalPayments().ToString("C"));

        AnsiConsole.Write(table);
    }

    void ISalable.AddSale(decimal saleAmount)
    {
        Sales.Add(saleAmount);
    }

    List<decimal> ISalable.GetSales()
    {
        return Sales;
    }

    public decimal GetTotalPayments()
    {
        return Sales.Sum();
    }

    public void AddPayment(decimal payment)
    {
        Sales.Add(payment);
    }

    public override string ToString()
    {
        return $"{FirstName} {LastName} - {Email} | Total Sales: {Sales.Count}, Total Payments: {GetTotalPayments():C}";
    }
}

class Employee : Person
{
    public decimal Salary { get; set; }

    public Employee(string firstName, string lastName, string email, decimal salary)
    {
        FirstName = firstName;
        LastName = lastName;
        Email = email;
        Salary = salary;
    }

    public override void DisplayInfo()
    {
        var table = new Table();
        table.AddColumn("First Name");
        table.AddColumn("Last Name");
        table.AddColumn("Email");
        table.AddColumn("Salary");

        table.AddRow(FirstName, LastName, Email, Salary.ToString("C"));
        AnsiConsole.Write(table);
    }

    public override string ToString()
    {
        return $"{FirstName} {LastName} - {Email} | Salary: {Salary:C}";
    }
}

class CRMSystem : ISearchable
{
    private List<Customer> customers = new();
    private List<Employee> employees = new();

    public void AddCustomer(Customer customer)
    {
        if (IsEmailDuplicate(customer.Email))
        {
            throw new DuplicateEmailException("This email is already in use.");
        }
        customers.Add(customer);
    }

    public void AddEmployee(Employee employee)
    {
        if (IsEmailDuplicate(employee.Email))
        {
            throw new DuplicateEmailException("This email is already in use.");
        }
        employees.Add(employee);
    }

    public void DisplayAllCustomers()
    {
        if (customers.Count == 0)
        {
            AnsiConsole.MarkupLine("[red]No customers found.[/]");
        }
        else
        {
            foreach (var customer in customers)
            {
                customer.DisplayInfo();
            }
        }
    }

    public void DisplayAllEmployees()
    {
        if (employees.Count == 0)
        {
            AnsiConsole.MarkupLine("[red]No employees found.[/]");
        }
        else
        {
            foreach (var employee in employees)
            {
                employee.DisplayInfo();
            }
        }
    }

    public void SearchCustomerByName(string name)
    {
        var foundCustomers = customers.FindAll(c => c.FirstName.Contains(name, StringComparison.OrdinalIgnoreCase) ||
                                                    c.LastName.Contains(name, StringComparison.OrdinalIgnoreCase) ||
                                                    c.Email.Contains(name, StringComparison.OrdinalIgnoreCase));
        if (foundCustomers.Count == 0)
        {
            AnsiConsole.MarkupLine("[red]No customers found.[/]");
        }
        else
        {
            foreach (var customer in foundCustomers)
            {
                AnsiConsole.WriteLine(customer.ToString());
            }
        }
    }

    public void Search(string query)
    {
        SearchCustomerByName(query);
    }

    public bool IsEmailDuplicate(string email)
    {
        return customers.Exists(c => c.Email.Equals(email, StringComparison.OrdinalIgnoreCase)) ||
               employees.Exists(e => e.Email.Equals(email, StringComparison.OrdinalIgnoreCase));
    }
}

public class DuplicateEmailException : Exception
{
    public DuplicateEmailException(string message) : base(message) { }
}

class Program
{
    static void Main(string[] args)
    {
        AnsiConsole.Write(
        new FigletText("CRM Management")
        .Centered()
        .Color(Color.Green)
        );
        Console.WriteLine();

        CRMSystem crm = new();

        while (true)
        {
            try
            {
                AnsiConsole.Write(new Markup("[bold yellow]Choose an option:[/]"));
                AnsiConsole.WriteLine();
                var choice = AnsiConsole.Prompt(
                    new SelectionPrompt<string>()
                        .Title("Select an [green]option[/]:")
                        .AddChoices("Add Customer", "Add Employee", "Display All Customers", "Display All Employees", "Search Customer by Name or Last name or Email", "Exit"));

                switch (choice)
                {
                    case "Add Customer":
                        string firstName = GetValidInput("Enter first name: ");
                        string lastName = GetValidInput("Enter last name: ");
                        string email = GetValidEmail("Enter email: ", crm);

                        Customer customer = new Customer(firstName, lastName, email);
                        crm.AddCustomer(customer);

                        decimal paymentAmount = GetValidDecimalInput("Enter payment amount: ");
                        customer.AddPayment(paymentAmount);

                        AnsiConsole.MarkupLine("[green]Customer added successfully![/]");
                        Console.WriteLine();
                        break;
                    case "Add Employee":
                        firstName = GetValidInput("Enter first name: ");
                        lastName = GetValidInput("Enter last name: ");
                        email = GetValidEmail("Enter email: ", crm);
                        decimal salary = GetValidDecimalInput("Enter salary: ");
                        crm.AddEmployee(new Employee(firstName, lastName, email, salary));
                        AnsiConsole.MarkupLine("[green]Employee added successfully![/]");
                        Console.WriteLine();
                        break;
                    case "Display All Customers":
                        crm.DisplayAllCustomers();
                        break;
                    case "Display All Employees":
                        crm.DisplayAllEmployees();
                        break;
                    case "Search Customer by Name":
                        string query = GetValidInput("Enter the first name, last name, or email to search: ");
                        crm.SearchCustomerByName(query);
                        break;
                    case "Exit":
                        return;
                    default:
                        AnsiConsole.MarkupLine("[red]Invalid option. Please try again.[/]");
                        break;
                }
            }
            catch (DuplicateEmailException ex)
            {
                AnsiConsole.MarkupLine($"[red]{ex.Message}[/]");
            }
            catch (Exception ex)
            {
                AnsiConsole.MarkupLine($"[red]An error occurred: {ex.Message}[/]");
            }
        }
    }

    static string GetValidInput(string prompt)
    {
        string input;
        do
        {
            AnsiConsole.Write(new Markup($"[bold]{prompt}[/]"));
            input = Console.ReadLine()!;
            if (string.IsNullOrWhiteSpace(input))
            {
                AnsiConsole.MarkupLine("[red]Input cannot be empty. Please try again.[/]");
            }
        } while (string.IsNullOrWhiteSpace(input));
        return input;
    }

    static string GetValidEmail(string prompt, CRMSystem crm)
    {
        string email;
        do
        {
            AnsiConsole.Write(new Markup($"[bold]{prompt}[/]"));
            email = Console.ReadLine()!;
            if (string.IsNullOrWhiteSpace(email) || !email.Contains("@") || crm.IsEmailDuplicate(email))
            {
                AnsiConsole.MarkupLine("[red]Invalid or duplicate email. Please enter a valid, unique email.[/]");
                email = "";
            }
        } while (string.IsNullOrWhiteSpace(email));
        return email;
    }

    static decimal GetValidDecimalInput(string prompt)
    {
        decimal result;
        do
        {
            AnsiConsole.Write(new Markup($"[bold]{prompt}[/]"));
            if (!decimal.TryParse(Console.ReadLine(), out result) || result <= 0)
            {
                AnsiConsole.MarkupLine("[red]Invalid amount. Please enter a valid number greater than 0.[/]");
            }
        } while (result <= 0);
        return result;
    }
}
