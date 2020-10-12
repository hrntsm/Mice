using System;
using System.Collections.Generic;
using System.Drawing;
using System.Runtime.CompilerServices;
using Grasshopper.Kernel;
using Rhino.Geometry;

namespace Mice.Components.Result
{
    public class MomentViewer : GH_Component {
        private readonly List<double> _param = new List<double>();
        private Point3d _mPoint1, _mPoint2, _mPoint3, _mPoint4, _mPoint5;
        private readonly List<Brep> _momentBrep = new List<Brep> ();
        private Rhino.Display.DisplayMaterial _momentMaterial;
        private readonly Color _textColour = Color.FromName("Black");
        private readonly Color _momentColour = Color.FromName("SkyBlue");

        protected override Bitmap Icon => Properties.Resource.Result_M_icon;
        public override Guid ComponentGuid => new Guid("419c3a3a-cc48-4717-8cef-5f5647a5dcAa");
        public MomentViewer()
            : base("Moment View", "Moment", "Display Moment", "Mice", "Result")
        {
        }
        
        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddNumberParameter("Moment", "M", "Input Moment", GH_ParamAccess.list, new List<double>(){0, 0, 0, 0, 0});
            pManager.AddNumberParameter("Scale", "Sc", "Input Output Scale", GH_ParamAccess.item, 10);
            pManager[0].Optional = true;
        }
        public override void ClearData()
        {
            base.ClearData();
            _param.Clear();
            _momentBrep.Clear();
        }
        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddBrepParameter("View Moment Surface", "Srf", "output Moment Surface", GH_ParamAccess.item);
        }
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            // 引数設定
            double Sc = double.NaN;

            // 入力設定
            if (!DA.GetDataList(0, _param)) { return; }
            if (!DA.GetData(1, ref Sc)) { return; }

            // Scが0だとbrepが作れずエラーが出るので、Scの最小値を1に設定
            if (Sc < 1)
                Sc = 1;
            
            if(_param.Count <= 1)
                return;
            
            // モーメント図の作成
            var M12_P1 = new Point3d(0, 0, 0);
            var M12_P2 = new Point3d(0, 0, Sc * -_param[0]);
            var M12_P3 = new Point3d(0, _param[5] / 4, Sc * -_param[1]);
            var M12_P4 = new Point3d(0, _param[5] / 4, 0);
            _mPoint1 = new Point3d(0, 0, Sc * -_param[0]);
            //
            var M23_P1 = new Point3d(0, _param[5] / 4, 0);
            var M23_P2 = new Point3d(0, _param[5] / 4, Sc * -_param[1]);
            var M23_P3 = new Point3d(0, _param[5] / 2, Sc * -_param[2]);
            var M23_P4 = new Point3d(0, _param[5] / 2, 0);
            _mPoint2 = new Point3d(0, _param[5] / 4, Sc * -_param[1]);
            //
            var M34_P1 = new Point3d(0, _param[5] / 2, 0);
            var M34_P2 = new Point3d(0, _param[5] / 2, Sc * -_param[2]);
            var M34_P3 = new Point3d(0, 3 * _param[5] / 4, Sc * -_param[3]);
            var M34_P4 = new Point3d(0, 3 * _param[5] / 4, 0);
            _mPoint3 = new Point3d(0, _param[5] / 2, Sc * -_param[2]);
            //
            var M45_P1 = new Point3d(0, 3 * _param[5] / 4, 0);
            var M45_P2 = new Point3d(0, 3 * _param[5] / 4, Sc * -_param[3]);
            var M45_P3 = new Point3d(0, _param[5], Sc * -_param[4]);
            var M45_P4 = new Point3d(0, _param[5], 0);
            _mPoint4 = new Point3d(0, 3 * _param[5] / 4, Sc * -_param[3]);
            _mPoint5 = new Point3d(0, _param[5], Sc * -_param[4]);

            Brep M12_brep = Brep.CreateFromCornerPoints(M12_P1, M12_P2, M12_P3, M12_P4, DocumentTolerance());
            Brep M23_brep = Brep.CreateFromCornerPoints(M23_P1, M23_P2, M23_P3, M23_P4, DocumentTolerance());
            Brep M34_brep = Brep.CreateFromCornerPoints(M34_P1, M34_P2, M34_P3, M34_P4, DocumentTolerance());
            Brep M45_brep = Brep.CreateFromCornerPoints(M45_P1, M45_P2, M45_P3, M45_P4, DocumentTolerance());

            // モデルはRhino上に出力するだけなので、とりあえず配列でまとめる
            _momentBrep.Add(M12_brep);
            _momentBrep.Add(M23_brep);
            _momentBrep.Add(M34_brep);
            _momentBrep.Add(M45_brep);

            // まとめての出力なので、SetDataList で出力
            DA.SetDataList(0, _momentBrep);
        }
        /// <summary>
        /// Rhino の viewport への出力
        /// </summary>
        public override void DrawViewportWires(IGH_PreviewArgs args)
        {
            if (_param.Count <= 1 || _param[1] == 0　)
                return;

            args.Display.Draw2dText(_param[0].ToString("F1"), _textColour, _mPoint1, true, 22);
            args.Display.Draw2dText(_param[1].ToString("F1"), _textColour, _mPoint2, true, 22);
            args.Display.Draw2dText(_param[2].ToString("F1"), _textColour, _mPoint3, true, 22);
            args.Display.Draw2dText(_param[3].ToString("F1"), _textColour, _mPoint4, true, 22);
            args.Display.Draw2dText(_param[4].ToString("F1"), _textColour, _mPoint5, true, 22);
            
            // surface 色付け
            _momentMaterial = new Rhino.Display.DisplayMaterial(_momentColour);

            for (int i = 0; i < 4; i++)
                args.Display.DrawBrepShaded(_momentBrep[i], _momentMaterial);
        }

        public override void DrawViewportMeshes(IGH_PreviewArgs args)
        {
            if (_param.Count <= 1 || _param[1] == 0　)
                return;

            _momentMaterial = new Rhino.Display.DisplayMaterial(_momentColour);
            for (int i = 0; i < 4; i++)
                args.Display.DrawBrepWires(_momentBrep[i], _momentMaterial.Diffuse, 0);
        }
    }
}