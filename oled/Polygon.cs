using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace oled
{

public class Polygon{
    public int npoints;        
    public int[] xpoints;      
    public int[] ypoints;      
    private double BIG_VALUE = double.MaxValue / 10.0;
    protected Rectangle bounds;

    bool converted = false;
    Point[] p;

    public Polygon()
    {
        xpoints = new int[4];
        ypoints = new int[4];
    }

    public Point[] toArray()
    {
        if(!converted){
            p = new Point[npoints];

            for(int i=0; i<npoints; i++)
                p[i] = new Point(xpoints[i], ypoints[i]);

            converted = true;
        }

        return p;
    }

    public void addPoint(int x, int y)
    {
        if(npoints + 1 > xpoints.Length){
            int[] newx = new int[npoints + 1];
            Array.Copy(xpoints, 0, newx, 0, npoints);
            xpoints = newx;
        }
        
        if(npoints + 1 > ypoints.Length){
            int[] newy = new int[npoints + 1];
            Array.Copy(ypoints, 0, newy, 0, npoints);
            ypoints = newy;
        }
        
        xpoints[npoints] = x;
        ypoints[npoints] = y;
        npoints++;
        
        if(bounds != null){
            if(npoints == 1){
                bounds.X = x;
                bounds.Y = y;
            }
            else{
                if(x < bounds.X){
                    bounds.Width += bounds.X - x;
                    bounds.X = x;
                }
                else if(x > bounds.X + bounds.Width){
                    bounds.Width = x - bounds.X;
                }
                
                if(y < bounds.Y){
                    bounds.Height += bounds.Y - y;
                    bounds.Y = y;
                }
                else if(y > bounds.Y + bounds.Height){
                    bounds.Height = y - bounds.Y;
                }
            }
        }
    }
    
    public bool inside(int x, int y)       
    {
        return((evaluateCrossings(x, y, false, BIG_VALUE) & 1) != 0);
    }

    private int evaluateCrossings(double x, double y, bool useYaxis, double distance)
    {
        double x0;
        double x1;
        double y0;
        double y1;
        double epsilon = 0.0;
        int crossings = 0;
        int[] xp;
        int[] yp;

        if(useYaxis){
            xp = ypoints;
            yp = xpoints;
            double swap;
            swap = y;
            y = x;
            x = swap;
        }
        else{
            xp = xpoints;
            yp = ypoints;
        }
        
        // Get a value which is small but not insignificant relative the path.
        epsilon = 1E-7;
        
        x0 = xp[0] - x;
        y0 = yp[0] - y;
        
        for(int i=1; i<npoints; i++){
            x1 = xp[i] - x;
            y1 = yp[i] - y;
            
            if(y0 == 0.0)
                y0 -= epsilon;
            
            if(y1 == 0.0)
                y1 -= epsilon;
            
            if(y0*y1 < 0)
                if(linesIntersect(x0, y0, x1, y1, epsilon, 0.0, distance, 0.0))
                    ++crossings;
            
            x0 = xp[i] - x;
            y0 = yp[i] - y;
        }
        
        // end segment
        x1 = xp[0] - x;
        y1 = yp[0] - y;
        
        if(y0 == 0.0)
            y0 -= epsilon;
            
        if(y1 == 0.0)
            y1 -= epsilon;
        
        if(y0*y1 < 0)
            if(linesIntersect(x0, y0, x1, y1, epsilon, 0.0, distance, 0.0))
                ++crossings;
        
        return crossings;
    }

    private static bool between(double x1, double y1, double x2, double y2, double x3, double y3) 
    {
        if(x1 != x2){
            return (x1<=x3 && x3<=x2) || (x1>=x3 && x3>=x2);   
        }
        else{
            return (y1<=y3 && y3<=y2) || (y1>=y3 && y3>=y2);   
        }
    }

    public static bool linesIntersect(double x1, double y1, double x2, double y2, double x3, double y3, double x4, double y4)
    {
        double a1, a2, a3, a4;
        
        // deal with special cases
        if((a1 = area2(x1, y1, x2, y2, x3, y3)) == 0.0){
            // check if p3 is between p1 and p2 OR p4 is collinear also AND either between p1 and p2 OR at opposite ends
            if(between(x1, y1, x2, y2, x3, y3)){
                return true;
            }
            else{
                if(area2(x1, y1, x2, y2, x4, y4) == 0.0){
                    return between(x3, y3, x4, y4, x1, y1) || between (x3, y3, x4, y4, x2, y2);
                }
                else{
                    return false;
                }
            }
        }
        else if((a2 = area2(x1, y1, x2, y2, x4, y4)) == 0.0){
            // check if p4 is between p1 and p2 (we already know p3 is not collinear)
            return between(x1, y1, x2, y2, x4, y4);
        }
        
        if((a3 = area2(x3, y3, x4, y4, x1, y1)) == 0.0){
            // check if p1 is between p3 and p4 OR p2 is collinear also AND either between p1 and p2 OR at opposite ends
            if(between(x3, y3, x4, y4, x1, y1)){
                return true;
            }
            else{
                if(area2(x3, y3, x4, y4, x2, y2) == 0.0){
                    return between(x1, y1, x2, y2, x3, y3) || between (x1, y1, x2, y2, x4, y4);
                }
                else{
                    return false;
                }
            }
        }
        else if((a4 = area2(x3, y3, x4, y4, x2, y2)) == 0.0){
            // check if p2 is between p3 and p4 (we already know p1 is not collinear)
            return between(x3, y3, x4, y4, x2, y2);
        }
        else{  // test for regular intersection
            return((a1 > 0.0) ^ (a2 > 0.0)) && ((a3 > 0.0) ^ (a4 > 0.0));
        }
    }

    private static double area2(double x1, double y1, double x2, double y2, double x3, double y3) 
    {
        return (x2-x1) * (y3-y1) - (x3-x1) * (y2-y1);    
    }
}

}
