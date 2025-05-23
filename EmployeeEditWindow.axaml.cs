using Avalonia.Controls;
using Avalonia.Input;
using EmployeeStruct.Models;
using System.Threading.Tasks;

namespace EmployeeStruct
{
    public partial class EmployeeEditWindow : Window
    {
        public Employee Employee { get; private set; } = new Employee();
        private User15Context db = new User15Context();

        public EmployeeEditWindow()
        {
            InitializeComponent();

        }
    }
}

