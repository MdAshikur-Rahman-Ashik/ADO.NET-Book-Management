using Ashik_1280982.ViewModel;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Ashik_1280982
{
    public partial class Form1 : Form
    {
        string conStr =
        ConfigurationManager.ConnectionStrings["con"].ConnectionString;
        int intBokId = 0;
        string strPreviousImage = "";
        bool defaultImage = true;
        OpenFileDialog ofd = new OpenFileDialog();

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            LoadCategoryCmb();
            LoaddgvBookList();
            Clear();
        }

        private void LoadCategoryCmb()
        {
            using (SqlConnection con = new SqlConnection(conStr))
            {
                con.Open();
                SqlDataAdapter sda = new SqlDataAdapter("SELECT * FROM Category", con);
                DataTable dt = new DataTable();
                sda.Fill(dt);
                DataRow topRow = dt.NewRow();
                topRow[0] = 0;
                topRow[1] = "--Select--";
                dt.Rows.InsertAt(topRow, 0);
                cmbCategory.ValueMember = "CategoryId";
                cmbCategory.DisplayMember = "CategoryTitle";
                cmbCategory.DataSource = dt;
            }
        }

        private void LoaddgvBookList()
        {
            using (SqlConnection con = new SqlConnection(conStr))
            {
                con.Open();
                SqlDataAdapter sda = new SqlDataAdapter("ViewAllBooks", con);
                sda.SelectCommand.CommandType = CommandType.StoredProcedure;
                DataTable dt = new DataTable();
                sda.Fill(dt);
                dt.Columns.Add("Image", Type.GetType("System.Byte[]"));
                foreach (DataRow dr in dt.Rows)
                {
                    dr["Image"] = File.ReadAllBytes(Application.StartupPath + "\\images\\" + dr["ImagePath"].ToString());
                }
                dgvBookList.RowTemplate.Height = 100;
                dgvBookList.DataSource = dt;

                ((DataGridViewImageColumn)dgvBookList.Columns[dgvBookList.Columns.Count - 1]).ImageLayout = DataGridViewImageCellLayout.Stretch;

                sda.Dispose();
            }
        }

        private void Clear()
        {

            txtBookCode.Text = "";
            txtBookName.Text = "";
            cmbCategory.SelectedIndex = 0;
            dtpPUB.Value = DateTime.Now;
            rbtnAvailable.Checked = true;
            chkType.Checked = true;
            intBokId = 0;
            btnDelete.Enabled = false;
            btnSave.Text = "Save";
            pictureBoxBook.Image = Image.FromFile(Application.StartupPath + "\\images\\noimage.jpg");
            defaultImage = true;
            if (dgvcategorywiseprice.DataSource == null)
            {
                dgvcategorywiseprice.Rows.Clear();
            }
            else
            {
                dgvcategorywiseprice.DataSource = (dgvcategorywiseprice.DataSource as DataTable).Clone();
            }
        }

        private void btnReset_Click(object sender, EventArgs e)
        {
            Clear();
        }

        private void btnBrowser_Click(object sender, EventArgs e)
        {
            ofd.Filter = "Images(.jpg,.png,.png)|*.png;*.jpg; *.png";
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                pictureBoxBook.Image = new Bitmap(ofd.FileName);
                if (intBokId == 0)
                {
                    defaultImage = false;
                    strPreviousImage = "";
                }

            }
        }

        private void btnClear_Click(object sender, EventArgs e)
        {
            pictureBoxBook.Image = new Bitmap(Application.StartupPath + "\\images\\noimage.jpg");
            defaultImage = true;
            strPreviousImage = "";
        }
        bool ValidateMasterDetailForm()
        {
            bool isValid = true;
            if (txtBookName.Text.Trim() == "")
            {
                MessageBox.Show("Book name is required");
                isValid = false;
            }
            return isValid;
        }
        string SaveImage(string imgPath)
        {
            string fileName = Path.GetFileNameWithoutExtension(imgPath);
            string ext = Path.GetExtension(imgPath);
            fileName = fileName.Length <= 15 ? fileName : fileName.Substring(0, 15);
            fileName = fileName + DateTime.Now.ToString("yymmssfff") + ext;
            pictureBoxBook.Image.Save(Application.StartupPath + "\\images\\" + fileName);
            return fileName;
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            if (ValidateMasterDetailForm())
            {
                int BokId = 0;
                using (SqlConnection con = new SqlConnection(conStr))
                {
                    con.Open();
                    SqlCommand cmd = new SqlCommand("BookAddOrEdit", con);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@BookId", intBokId);
                    cmd.Parameters.AddWithValue("@BookCode", txtBookCode.Text.Trim());
                    cmd.Parameters.AddWithValue("@BookName", txtBookName.Text.Trim());
                    cmd.Parameters.AddWithValue("@CategoryId", Convert.ToInt16(cmbCategory.SelectedValue));
                    cmd.Parameters.AddWithValue("@PublishDate", dtpPUB.Value);
                    cmd.Parameters.AddWithValue("@IsMultiLanguage", chkType.Checked ? "True" : "False");
                    cmd.Parameters.AddWithValue("@BookStatus", rbtnAvailable.Checked ? "Available" : "CheckedOut");
                    if (defaultImage)
                    {
                        cmd.Parameters.AddWithValue("@ImagePath", DBNull.Value);
                    }

                    else if (intBokId > 0 && strPreviousImage != "")
                    {
                        cmd.Parameters.AddWithValue("@ImagePath", strPreviousImage);
                        //if(ofd.FileName!= strPreviousImage)
                        //{
                        //    var filename = Application.StartupPath + "\\images\\" + strPreviousImage;
                        //    if (pictureBoxEmployee.Image != null)
                        //    {
                        //        pictureBoxEmployee.Image.Dispose();
                        //        pictureBoxEmployee.Image = null;
                        //        System.IO.File.Delete(filename);
                        //    }
                        //}

                    }
                    else
                    {
                        cmd.Parameters.AddWithValue("@ImagePath", SaveImage(ofd.FileName));
                    }
                    BokId = Convert.ToInt16(cmd.ExecuteScalar());
                }
                using (SqlConnection con = new SqlConnection(conStr))
                {
                    con.Open();
                    foreach (DataGridViewRow item in dgvcategorywiseprice.Rows)
                    {
                        if (item.IsNewRow) break;
                        else
                        {
                            SqlCommand cmd = new SqlCommand("BookCategoryWisePriceAddAndEdit", con);
                            cmd.CommandType = CommandType.StoredProcedure;
                            cmd.Parameters.AddWithValue("@CategoryWisePriceId", Convert.ToInt32(item.Cells["CategoryWisePriceId"].Value == DBNull.Value ? "0" : item.Cells["CategoryWisePriceId"].Value));
                            cmd.Parameters.AddWithValue("@BookId", BokId);
                            cmd.Parameters.AddWithValue("@CategoryName", item.Cells["CategoryName"].Value);
                            cmd.Parameters.AddWithValue("@Price", item.Cells["Price"].Value);
                            cmd.ExecuteNonQuery();
                        }
                    }
                }
                LoaddgvBookList();
                Clear();
                MessageBox.Show("Submitted Successfully");
            }
        }

        private void dgvBookList_DoubleClick(object sender, EventArgs e)
        {
            if (dgvBookList.CurrentRow.Index != -1)
            {
                DataGridViewRow dgvRow = dgvBookList.CurrentRow;
                intBokId = Convert.ToInt32(dgvRow.Cells[0].Value);
                using (SqlConnection con = new SqlConnection(conStr))
                {
                    con.Open();
                    SqlDataAdapter sda = new SqlDataAdapter("ViewBookByBookId", con);
                    sda.SelectCommand.CommandType = CommandType.StoredProcedure;
                    sda.SelectCommand.Parameters.AddWithValue("@BookId", intBokId);
                    DataSet ds = new DataSet();
                    sda.Fill(ds);
                    //--Master---
                    DataRow dr = ds.Tables[0].Rows[0];
                    txtBookCode.Text = dr["BookCode"].ToString();
                    txtBookName.Text = dr["BookName"].ToString();
                    cmbCategory.SelectedValue = Convert.ToInt32(dr["CategoryId"].ToString());
                    dtpPUB.Value = Convert.ToDateTime(dr["PublishDate"].ToString());
                    if (Convert.ToBoolean(dr["IsMultiLanguage"].ToString()))
                    {
                        chkType.Checked = true;
                    }
                    else
                    {
                        chkType.Checked = false;
                    }
                    if ((dr["BookStatus"].ToString().Trim()) == "Available")
                    {
                        rbtnAvailable.Checked = true;
                    }
                    else
                    {
                        rbtnAvailable.Checked = false;
                    }
                    if ((dr["BookStatus"].ToString().Trim()) == "CheckedOut")
                    {
                        rbtnAvailable.Checked = true;
                    }
                    else
                    {
                        rbtnCheckedOut.Checked = false;
                    }
                    if (dr["ImagePath"] == DBNull.Value)
                    {
                        pictureBoxBook.Image = new Bitmap(Application.StartupPath + "\\images\\noimage.jpg");
                    }
                    else
                    {
                        string image = dr["ImagePath"].ToString();
                        pictureBoxBook.Image = new Bitmap(Application.StartupPath + "\\images\\" + dr["ImagePath"].ToString());
                        strPreviousImage = dr["ImagePath"].ToString();
                        defaultImage = false;
                    }
                    //--Details---
                    dgvcategorywiseprice.AutoGenerateColumns = false;
                    dgvcategorywiseprice.DataSource = ds.Tables[1];
                    btnDelete.Enabled = true;
                    btnSave.Text = "Update";
                    tabControl1.SelectedIndex = 0;
                }
            }
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Are you sure to delete this record?", "Master Details", MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                string image = "";
                using (SqlConnection con = new SqlConnection(conStr))
                {
                    con.Open();
                    SqlDataAdapter sda = new SqlDataAdapter("ViewBookByBookId", con);
                    sda.SelectCommand.CommandType = CommandType.StoredProcedure;
                    sda.SelectCommand.Parameters.AddWithValue("@BookId", intBokId);
                    DataSet ds = new DataSet();
                    sda.Fill(ds);
                    DataRow dr = ds.Tables[0].Rows[0];
                    if (dr["ImagePath"] != DBNull.Value)
                    {
                        image = dr["ImagePath"].ToString();
                        var filename = Application.StartupPath + "\\images\\" + image;
                        if (pictureBoxBook.Image != null)
                        {
                            pictureBoxBook.Image.Dispose();
                            pictureBoxBook.Image = null;
                            System.IO.File.Delete(filename);
                        }

                    }
                    SqlCommand cmd = new SqlCommand("BookCategoryWisePriceDelete", con);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@BookId", intBokId);
                    sda.Dispose();
                    cmd.ExecuteNonQuery();
                    LoaddgvBookList();
                    Clear();
                    MessageBox.Show("Deleted Successfully");
                }
                // File.Delete(filePath);
            }
        }

        private void dgvcategorywiseprice_UserDeletingRow(object sender, DataGridViewRowCancelEventArgs e)
        {
            DataGridViewRow dgvRow = dgvcategorywiseprice.CurrentRow;
            if (dgvRow.Cells["CategoryWisePriceId"].Value != DBNull.Value)
            {
                if (MessageBox.Show("Are you sure to delete this record?", "Master Details", MessageBoxButtons.YesNo) == DialogResult.Yes)
                {
                    using (SqlConnection con = new SqlConnection(conStr))
                    {
                        con.Open();
                        SqlCommand cmd = new SqlCommand("CategoryWisePriceDelete", con);
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@CategoryId", dgvRow.Cells["CategoryWisePriceId"].Value);
                        cmd.ExecuteNonQuery();
                    }

                }
                else
                {
                    e.Cancel = true;
                }
            }
        }

        private void btnViewDetails_Click(object sender, EventArgs e)
        {
            using (SqlConnection con = new SqlConnection(conStr))
            {
                con.Open();
                SqlDataAdapter sda = new SqlDataAdapter("ViewAllBooks", con);
                sda.SelectCommand.CommandType = CommandType.StoredProcedure;
                DataTable dt = new DataTable();
                sda.Fill(dt);
                List<BookViewModel> list = new List<BookViewModel>();
                BookViewModel BookVm;
                if (dt.Rows.Count > 0)
                {
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        BookVm = new BookViewModel();
                        BookVm.BookId = Convert.ToInt32(dt.Rows[i]["BookId"]);
                        BookVm.BookCode = dt.Rows[i]["BookCode"].ToString();
                        BookVm.BookName = dt.Rows[i]["BookName"].ToString();
                        BookVm.PublishDate = Convert.ToDateTime(dt.Rows[i]["PublishDate"].ToString());
                        BookVm.BookStatus = dt.Rows[i]["BookStatus"].ToString();
                        BookVm.IsMultiLanguage = Convert.ToBoolean(dt.Rows[i]["IsMultiLanguage"].ToString());
                        BookVm.TotalCategoryWisePrice = Convert.ToInt32(dt.Rows[i]["TotalCategoryWisePrice"]);
                        BookVm.CategoryTitle = dt.Rows[i]["CategoryTitle"].ToString();
                        BookVm.ImagePath = Application.StartupPath + "\\images\\" + dt.Rows[i]["ImagePath"].ToString();
                        list.Add(BookVm);

                    }
                    using (BookReport report = new BookReport(list))
                    {
                        report.ShowDialog();
                    }
                }


            }
        }
    }
}
