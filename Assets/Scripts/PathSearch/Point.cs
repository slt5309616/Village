using System;
using System.Collections.Generic;
using System.Text;

  public  class Point {
        public int y;
        public int x;
        public int G;
        public int H;

        public bool isVisited = false;
      /// <summary>
      ///   标记是否在OpenList中，为了避免遍历OpenList来寻找
      /// </summary>
        public bool isInOpenList = false;
      /// <summary>
      ///   标记是否可到达
      /// </summary>
        public bool isActive;
        public Point() {
        }
        public Point(int x,int y) {
            this.x = x;
            this.y = y;
        }
        public Point(int x0, int y0, int G0, int H0, Point F) {
            x = x0;
            y = y0;
            G = G0;
            H = H0;
            father = F;
        }


        public Point father;
    }
