using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace DPDManager
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            ReloadRule();
        }


        private void ListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var _dvm= (DbViewModel)this.DataContext;
            tbCode.Text = "";
            tbCode.AppendText(_dvm.GetTableCode(lbTables.SelectedItems));
        }

        //添加规则
        private void btAddRule(object sender, RoutedEventArgs e)
        {
            var _db = (DbViewModel)this.DataContext;
            bool bSegType = lbSegtype.SelectedItems.Count > 0;
            bool bProperty = lbPropertyName.SelectedItems.Count > 0;
            bool bPValue = lbPropertyValue.SelectedItems.Count > 0;
            bool bValueType = lbValueType.SelectedItems.Count > 0;
            bool bIsNecessary = (cbIsNeccessary.IsChecked == true);
            if(bIsNecessary && bSegType && bProperty && !bPValue && !bValueType)
            {
                foreach(ISItem segtype in lbSegtype.SelectedItems)
                {
                    foreach(string property in lbPropertyName.SelectedItems)
                    {
                         _db.Execute(string.Format("INSERT INTO rule_segtype_necessaryproperty (segtype, propertytoken) VALUES({0}, '{1}');",
                            segtype.IntValue, property));
                    }
                }

            }
            else if(!bIsNecessary && bSegType && bProperty && !bPValue && !bValueType)
            {
                foreach (ISItem segtype in lbSegtype.SelectedItems)
                {
                    foreach (string property in lbPropertyName.SelectedItems)
                    {
                        _db.Execute(string.Format("INSERT INTO rule_segtype_compatibleproperty (segtype, propertytoken) VALUES({0}, '{1}');",
                           segtype.IntValue, property));
                    }
                }
            }
            else if(bSegType && bProperty && !bPValue && bValueType && (lbSegtype.SelectedItems.Count == 1) && (lbPropertyName.SelectedItems.Count == 1) && (lbValueType.SelectedItems.Count == 1))
            {
                //rule_segtype_property_vtype
                string sql = string.Format("INSERT INTO rule_segtype_property_vtype( segtype, propertytoken, valuetype ) VALUES({0}, '{1}', {2});",
                    ((ISItem)(lbSegtype.SelectedItems[0])).IntValue, lbPropertyName.SelectedItems[0].ToString(), ((ISItem)(lbValueType.SelectedItems[0])).IntValue);
                _db.Execute(sql);

            }
            else if(!bSegType && bProperty && bPValue && !bValueType)
            {
                foreach(var prop in lbPropertyName.SelectedItems)
                {
                    foreach(var pvtoken in lbPropertyValue.SelectedItems)
                    {
                        string sql = string.Format("INSERT INTO rule_property_vtoken ( propertytoken, valuetoken ) VALUES ('{0}','{1}');",
                           prop,pvtoken );
                        _db.Execute(sql);
                    }
                }
            }
            else if(!bSegType && bProperty && !bPValue && bValueType)
            {
                foreach(var prop in lbPropertyName.SelectedItems)
                {
                    foreach(ISItem vtype in lbValueType.SelectedItems)
                    {
                        string sql = string.Format("INSERT INTO rule_property_vtype (propertytoken, valuetype) VALUES ('{0}',{1});",
                           prop, vtype.IntValue );
                        _db.Execute(sql);
                    }
                }
            }
            else
            {
                MessageBox.Show("NULL rule create");
            }
            ReloadRule();
        }

        private void ReloadRule()
        {
            lbRules.ItemsSource = ((DbViewModel)this.DataContext).GetRuleList();
        }

        //删除规则
        private void brRemoveRule(object sender, RoutedEventArgs e)
        {
            var _db = (DbViewModel)this.DataContext;
            foreach (Rule rl in lbRules.SelectedItems)
            {
                _db.RemoveRule(rl);
            }
            ReloadRule();
        }
    }
}
