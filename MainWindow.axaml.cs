using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using EmployeeStruct.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using Avalonia.Data;
using System.Linq;
using System.Runtime.CompilerServices;
using System;
using System.ComponentModel;

namespace EmployeeStruct
{
    public partial class MainWindow : Window
    {
        private User15Context db = new User15Context();
        private Dictionary<int, bool> departmentStates = new Dictionary<int, bool>();
        private Dictionary<int, List<Border>> departmentChildren = new Dictionary<int, List<Border>>();
        private Dictionary<int, List<Line>> departmentLines = new Dictionary<int, List<Line>>();

        private IEnumerable<Employee> _employees;
        public event PropertyChangedEventHandler? PropertyChanged;

        public IEnumerable<Employee> Employees
        {
            get => _employees;
            set
            {
                if (_employees != value)
                {
                    _employees = value;
                    OnPropertyChanged();
                }
            }
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }



        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;
            Loaded += MainWindow_Loaded;
#if DEBUG
            this.AttachDevTools();
#endif
        }




        private void ShowSubdivisionEmployees(DepartmentSubdivision subdivision)
        {
            Employees = db.Employees
                .Where(e => e.Subdivisionid == subdivision.Subdivisionid)
                .Include(e => e.Position)
                .Include(e => e.Subdivision)
                .OrderBy(e => e.Lastname)
                .AsEnumerable();

            if (Employees.Any())
            {
                EmployeesGrid.ScrollIntoView(Employees.First(), null);
            }
        }


        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            LoadOrganizationStructure();
            LoadAllEmployees();
        }

        private void LoadOrganizationStructure()
        {
            OrgCanvas.Children.Clear();
            departmentChildren.Clear();
            departmentLines.Clear();
            departmentStates.Clear();

            var departments = db.Departments
                .Include(d => d.DepartmentSubdivisions)
                .OrderBy(d => d.Departmentname)
                .ToList();

            double y = 30;
            foreach (var department in departments)
            {
                var deptBlock = CreateDepartmentBlock(department, 30, y);
                OrgCanvas.Children.Add(deptBlock);
                y += 80; 
            }
        }


        private Border CreateDepartmentBlock(Department department, double x, double y)
        {
            var border = new Border
            {
                Width = 250,
                MinHeight = 60,
                Background = Brushes.LightBlue,
                CornerRadius = new CornerRadius(8),
                BorderBrush = Brushes.DarkBlue,
                BorderThickness = new Thickness(1.5),
                Padding = new Thickness(10),
                Child = new TextBlock
                {
                    Text = department.Departmentname,
                    TextWrapping = TextWrapping.Wrap,
                    FontWeight = FontWeight.Bold,
                    FontSize = 14,
                    TextAlignment = TextAlignment.Center,
                    VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center
                },
                [Canvas.LeftProperty] = x,
                [Canvas.TopProperty] = y
            };

            border.PointerPressed += (s, e) => ToggleDepartmentVisibility(department);
            return border;
        }

        private void ToggleDepartmentVisibility(Department department)
        {
            if (!departmentStates.TryGetValue(department.Departmentid, out bool isExpanded))
                isExpanded = false;

            if (isExpanded)
                CollapseDepartment(department);
            else
                ExpandDepartment(department);

            departmentStates[department.Departmentid] = !isExpanded;
        }


        private void ExpandDepartment(Department department)
        {
            var subs = db.DepartmentSubdivisions
                .Where(s => s.Departmentid == department.Departmentid)
                .OrderBy(s => s.Subdivisionname)
                .ToList();

            var parentBlock = OrgCanvas.Children.OfType<Border>()
                .First(b => (b.Child as TextBlock)?.Text == department.Departmentname);

            double startY = parentBlock.Bounds.Top + parentBlock.Bounds.Height + 20;
            double fixedBlockHeight = 70; 

            foreach (var sub in subs)
            {
                var subBlock = CreateSubdivisionBlock(sub, parentBlock.Bounds.Left + 40, startY);
                OrgCanvas.Children.Add(subBlock);
                var line = DrawConnection(parentBlock, subBlock);

                subBlock.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
                subBlock.Arrange(new Rect(subBlock.DesiredSize));

                if (!departmentChildren.ContainsKey(department.Departmentid))
                {
                    departmentChildren[department.Departmentid] = new List<Border>();
                    departmentLines[department.Departmentid] = new List<Line>();
                }

                departmentChildren[department.Departmentid].Add(subBlock);
                departmentLines[department.Departmentid].Add(line);
                startY += fixedBlockHeight + 20; 
            }
        }

        private void CollapseDepartment(Department department)
        {
            if (departmentChildren.TryGetValue(department.Departmentid, out var children))
            {
                foreach (var child in children)
                {
                    OrgCanvas.Children.Remove(child);
                }
                children.Clear();
            }

            if (departmentLines.TryGetValue(department.Departmentid, out var lines))
            {
                foreach (var line in lines)
                {
                    OrgCanvas.Children.Remove(line);
                }
                lines.Clear();
            }
        }

        private Border CreateSubdivisionBlock(DepartmentSubdivision subdivision, double x, double y)
        {
            var border = new Border
            {
                Width = 220,
                MinHeight = 50,
                Background = Brushes.LightGreen,
                CornerRadius = new CornerRadius(6),
                BorderBrush = Brushes.DarkGreen,
                BorderThickness = new Thickness(1.2),
                Padding = new Thickness(8),
                Child = new TextBlock
                {
                    Text = subdivision.Subdivisionname,
                    TextWrapping = TextWrapping.Wrap,
                    FontSize = 13,
                    TextAlignment = TextAlignment.Center,
                    VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center
                },
                [Canvas.LeftProperty] = x,
                [Canvas.TopProperty] = y
            };

            border.PointerPressed += (s, e) => ShowSubdivisionEmployees(subdivision);
            return border;
        }

        private Line DrawConnection(Control from, Control to)
        {
            var line = new Line
            {
                StartPoint = new Point(
                    from.Bounds.Left + from.Bounds.Width / 2,
                    from.Bounds.Bottom),
                EndPoint = new Point(
                    to.Bounds.Left + to.Bounds.Width / 2,
                    to.Bounds.Top),
                Stroke = Brushes.Gray,
                StrokeThickness = 1.5,
                StrokeDashArray = new Avalonia.Collections.AvaloniaList<double> { 4, 2 }, 
                ZIndex = -1
            };

            OrgCanvas.Children.Add(line);
            return line;
        }

        private void LoadAllEmployees()
        {
            EmployeesGrid.ItemsSource = db.Employees
                .Include(e => e.Position)
                .Include(e => e.Subdivision)
                .OrderBy(e => e.Lastname)
                .ToList();
        }



        private async void AddEmployee_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new EmployeeEditWindow();
            if (await dialog.ShowDialog<bool>(this))
            {
                db.Employees.Add(dialog.Employee);
                db.SaveChanges();
                LoadAllEmployees();
            }
        }

        private void Refresh_Click(object sender, RoutedEventArgs e)
        {
            LoadOrganizationStructure();
            LoadAllEmployees();
        }


        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            var searchText = SearchBox.Text?.ToLower();
            Employees = db.Employees
                .Include(e => e.Position)
                .Include(e => e.Subdivision)
                .Where(emp =>
                    emp.Firstname.ToLower().Contains(searchText) ||
                    emp.Lastname.ToLower().Contains(searchText) ||
                    emp.Position.Positionname.ToLower().Contains(searchText) ||
                    emp.Subdivision.Subdivisionname.ToLower().Contains(searchText))
                .AsEnumerable();
        }
    }
}