using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Newtonsoft.Json.Linq;

namespace GelDesk.UI
{
    public class DataGridController : CompositeViewController<Control>
    {
        DataGridView _grid;
        Splitter _splitter;

        protected override void OnInitialize(RpcContext rpc, ComponentInfo info, ComponentFrameReader reader)
        {
            _grid = new DataGridView();
            Views.Add(_grid);
            _splitter = _grid.AttachDockSplitter(Views, ObjectData);
            ((System.ComponentModel.ISupportInitialize)(_grid)).BeginInit();
        }
        protected override void OnLoad(RpcContext rpc)
        {
            _grid.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;

            _grid.Name = Name;
            _grid.SetDockStyle(ObjectData, _splitter);

            var colArray = (JArray)ObjectData["columns"];
            if (colArray != null && colArray.Count > 0)
            {
                var gridCols = colArray.Select(token => (string)token)
                    .Select(text => new DataGridViewTextBoxColumn() {
                        HeaderText = text,
                        Name = text
                    })
                    .ToArray();
                _grid.Columns.AddRange(gridCols);
            }
            _grid.Size = new System.Drawing.Size(80, 140);
            _grid.MinimumSize = new System.Drawing.Size(80, 120);
        }
        protected override void OnLoadCompleted(RpcContext rpc)
        {
            ((System.ComponentModel.ISupportInitialize)(_grid)).EndInit();
        }
    }
}
