using Ashik_1280982.ViewModel;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Ashik_1280982
{
    public partial class BookReport : Form
    {
        List<BookViewModel> _list;
        public BookReport(List<BookViewModel>list)
        {
            InitializeComponent();
            _list = list;
        }

        private void BookReport_Load(object sender, EventArgs e)
        {
            RptBookInfo rpt = new RptBookInfo();
            rpt.SetDataSource(_list);
            crystalReportViewer1.ReportSource = rpt;
            crystalReportViewer1.Refresh();
        }
    }
}
