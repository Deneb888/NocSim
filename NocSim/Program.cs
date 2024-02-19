using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace NocSim
{
    public class Coordinate
    {
        public int x;
        public int y;

        public Coordinate(int a, int b)
        {
            x = a;
            y = b;
        }
    };

    public enum Orientation
    {
        North,
        South,
        West,
        East,
        nowhere
    };

    public static class Stat
    {
        public static int[] dropped = new int[480];
        public static int[] absorbed = new int[480];
        public static int[] life = new int[480];
        public static int[] distance = new int[480];
        public static int[] injected = new int[480];
        public static int[] missed = new int[480];

        public static double drop_rate, absorb_rate;
        public static int total_life, total_distance, total_dropped, total_absorbed, mess_injected;
        public static double average_distance, efficiency;

        public static List<Coordinate> droploc;

        public static void Init()
        {
            for (int i = 0; i < 480; i++)
            {
                dropped[i] = 0;
                absorbed[i] = 0;
                injected[i] = 0;
                missed[i] = 0;
            }

            mess_injected = 0;
            droploc = new List<Coordinate>();
        }

        public static void Calc()
        {
            total_dropped = 0;
            total_absorbed = 0;
            total_life = total_distance = 0;

            for (int i = 0; i < 480; i++)
            {
                if(dropped[i] > 0) total_dropped++;
                if (absorbed[i] > 0) total_absorbed++;
                total_life += life[i];
                total_distance += distance[i];

                if (absorbed[i] == 0 && dropped[i] == 0)
                    missed[i] = 1;
            }

            drop_rate = (double)total_dropped / (double)mess_injected;
            absorb_rate = (double)total_absorbed / (double) mess_injected;
            average_distance = (double)total_distance / (double)total_absorbed;
            efficiency = (double)total_distance / (double) total_life;
        }
    };    

    public class Message
    {
        public int Id;

        public int cnt;

        public int dis;

        // public Coordinate Location;
        public Coordinate Destination;

        public Message(int id)
        {
            Id = id;
            cnt = 0;
            dis = -1;
        }
    };

    public class PortType
    {
        // public NodeType Parent;

        public Orientation Orient;

        public Message Input;
        public Message Output;

        public PortType()
        {

        }
    };

    public class NodeType
    {
        public PortType[] Port;

        public Coordinate Location;

        // public int Id;

        public NodeType northNeighbour, southNeighbour, westNeighbour, eastNeighbour;

        public NodeType (int id, int x, int y)
        {
            // Id = id;

            Port = new PortType[4];

            Location = new Coordinate(x, y);

            for(int i=0; i<4; i++)
            {
                Port[i] = new PortType();
                Port[i].Orient = (Orientation)i;
            }
        }

        public void AddNeighbour(ref NodeType node, Orientation orient)
        {
            switch(orient)
            {
                case Orientation.North:
                    northNeighbour = node;
                    break;

                case Orientation.South:
                    southNeighbour = node;
                    break;

                case Orientation.East:
                    eastNeighbour = node;
                    break;

                case Orientation.West:
                    westNeighbour = node;
                    break;

            }
        }

        public void AddInputMessage(Message mess, Orientation orient)
        {
            Port[(int)orient].Input = mess;

            int offset_x = Math.Abs(mess.Destination.x - Location.x);
            int offset_y = Math.Abs(mess.Destination.y - Location.y);

            if(mess.dis < 0) mess.dis = offset_x + offset_y;

            String s;
            s = "Message "+ mess.Id.ToString() + " sent to node(" + Location.x.ToString() + " " + Location.y.ToString() + ") port " + orient.ToString();
            Console.WriteLine(s);
        }

        public void RemoveInputMessage(Orientation orient)
        {
            Port[(int)orient].Input = null;
        }

        public void AddOutputMessage(Message mess, Orientation orient)
        {
            Port[(int)orient].Output = mess;
        }

        public void RemoveOutputMessage(Orientation orient)
        {
            Port[(int)orient].Output = null;
        }

        public void Route()
        {
            for(int i=0; i<4; i++)
            {
                if(Port[i].Input != null)
                {
                    RouteMessage_Deflect3(Port[i].Input, i);
                }
            }
        }

        public void RouteMessage_old(Message mess, int porti)
        {
            int offset_x = mess.Destination.x - Location.x;
            int offset_y = mess.Destination.y - Location.y;

            if(offset_x > 0)
            {
                Port[(int)Orientation.East].Output = mess;
                Port[porti].Input = null;

                string s = "Message " + mess.Id.ToString() + " routed to Node (" + Location.x.ToString() + " " + Location.y.ToString() + ") East output";
                Console.WriteLine(s);
            }
            else if(offset_x < 0)
            {
                Port[(int)Orientation.West].Output = mess;
                Port[porti].Input = null;
                string s = "Message " + mess.Id.ToString() + " routed to Node (" + Location.x.ToString() + " " + Location.y.ToString() + ") West output";
                Console.WriteLine(s);
            }
            else
            {
                if(offset_y > 0)
                {
                    Port[(int)Orientation.North].Output = mess;
                    Port[porti].Input = null;
                    string s = "Message " + mess.Id.ToString() + " routed to Node (" + Location.x.ToString() + " " + Location.y.ToString() + ") North output";
                    Console.WriteLine(s);
                }
                else if (offset_y < 0)
                {
                    Port[(int)Orientation.South].Output = mess;
                    Port[porti].Input = null;
                    string s = "Message " + mess.Id.ToString() + " routed to Node (" + Location.x.ToString() + " " + Location.y.ToString() + ") South output";
                    Console.WriteLine(s);
                }
                else
                {
                    Port[porti].Input = null;
                }
            }
        }

        public void RouteMessage(Message mess, int porti)
        {
            int offset_x = mess.Destination.x - Location.x;
            int offset_y = mess.Destination.y - Location.y;

            Orientation target_orient = Orientation.East;
            bool canroute = false;

            if (offset_x > 0)
            {
                target_orient = Orientation.East;
                canroute = true;
            }
            else if (offset_x < 0)
            {
                target_orient = Orientation.West;
                canroute = true;
            }
            else
            {
                if (offset_y > 0)
                {
                    target_orient = Orientation.North;
                    canroute = true;
                }
                else if (offset_y < 0)
                {
                    target_orient = Orientation.South;
                    canroute = true;
                }
                else
                {
                    Port[porti].Input = null;
                }
            }

            if (Port[(int)target_orient].Output != null)
                canroute = false;

            if (canroute)
            {
                Port[(int)target_orient].Output = mess;
                
                string s = "Message " + mess.Id.ToString() + " routed to Node (" + Location.x.ToString() + " " + Location.y.ToString() + ")" + target_orient.ToString() + "output";
                Console.WriteLine(s);
            }
            else
            {
                string s = "Message " + mess.Id.ToString() + " cannot route";
                Console.WriteLine(s);
            }

            Port[porti].Input = null;
        }

        public void RouteMessage_Deflect(Message mess, int porti)
        {
            int offset_x = mess.Destination.x - Location.x;
            int offset_y = mess.Destination.y - Location.y;

            Orientation target_orient = Orientation.nowhere;
            Orientation deflect_orient = Orientation.nowhere;

            bool canroute = false;
            bool absorbed = false;
            bool need_deflect = false;

            if (offset_x > 0)
            {
                target_orient = Orientation.East;
                canroute = true;

                if(offset_y > 0) {
                    deflect_orient = Orientation.North;
                }
                else if(offset_y < 0)
                {
                    deflect_orient = Orientation.South;
                }
                else
                {
                    deflect_orient = Orientation.North;
                }
            }
            else if (offset_x < 0)
            {
                target_orient = Orientation.West;
                canroute = true;

                if (offset_y > 0)
                {
                    deflect_orient = Orientation.North;
                }
                else if (offset_y < 0)
                {
                    deflect_orient = Orientation.South;
                }
                else
                {
                    deflect_orient = Orientation.North;
                }
            }
            else
            {
                if (offset_y > 0)
                {
                    target_orient = Orientation.North;
                    canroute = true;

                    if (offset_x > 0)
                    {
                        deflect_orient = Orientation.East;
                    }
                    else if (offset_x < 0)
                    {
                        deflect_orient = Orientation.West;
                    }
                    else
                    {
                        deflect_orient = Orientation.West;
                    }
                }
                else if (offset_y < 0)
                {
                    target_orient = Orientation.South;
                    canroute = true;

                    if (offset_x > 0)
                    {
                        deflect_orient = Orientation.East;
                    }
                    else if (offset_x < 0)
                    {
                        deflect_orient = Orientation.West;
                    }
                    else
                    {
                        deflect_orient = Orientation.West;
                    }
                }
                else
                {
                    Port[porti].Input = null;
                    absorbed = true;
                }
            }

            if (target_orient == Orientation.nowhere)
                canroute = false;
            else if (Port[(int)target_orient].Output != null)
            {
                canroute = false;
                need_deflect = true;
            }

            if (canroute)
            {
                Port[(int)target_orient].Output = mess;

                string s = "Message " + mess.Id.ToString() + " routed to Node (" + Location.x.ToString() + " " + Location.y.ToString() + ")" + target_orient.ToString() + "output";
                Console.WriteLine(s);
            }
            else if (need_deflect && deflect_orient != Orientation.nowhere)
            {
                Port[(int)deflect_orient].Output = mess;

                string s = "Message " + mess.Id.ToString() + " defelcted to Node (" + Location.x.ToString() + " " + Location.y.ToString() + ")" + deflect_orient.ToString() + "output";
                Console.WriteLine(s);
            }
            else if(absorbed)
            {
                string s = "Message " + mess.Id.ToString() + " absorbed";
                Console.WriteLine(s);
            }
            else
            {
                string s = "Message " + mess.Id.ToString() + " cannot route" + offset_x.ToString() + offset_y.ToString();
                Console.WriteLine(s);
            }

            Port[porti].Input = null;
        }

        public void RouteMessage_Deflect2(Message mess, int porti)
        {
            int offset_x = mess.Destination.x - Location.x;
            int offset_y = mess.Destination.y - Location.y;

            Orientation target_orient = Orientation.nowhere;
            Orientation deflect_orient = Orientation.nowhere;

            bool canroute = false;
            bool absorbed = false;
            bool need_deflect = false;

            if (offset_x > 0)
            {
                target_orient = Orientation.East;
                canroute = true;

                deflect_orient = DeflectY(offset_y);
            }
            else if (offset_x < 0)
            {
                target_orient = Orientation.West;
                canroute = true;

                deflect_orient = DeflectY(offset_y);
            }
            else
            {
                if (offset_y > 0)
                {
                    target_orient = Orientation.North;
                    canroute = true;

                    deflect_orient = DeflectX(offset_x);
                }
                else if (offset_y < 0)
                {
                    target_orient = Orientation.South;
                    canroute = true;

                    deflect_orient = DeflectX(offset_x);
                }
                else
                {
                    Port[porti].Input = null;
                    absorbed = true;
                }
            }

            if (target_orient == Orientation.nowhere)
                canroute = false;
            else if (Port[(int)target_orient].Output != null)
            {
                canroute = false;
                need_deflect = true;
            }

            if (canroute)
            {
                Port[(int)target_orient].Output = mess;
                mess.cnt++;

                string s = "Message " + mess.Id.ToString() + " routed to Node (" + Location.x.ToString() + " " + Location.y.ToString() + ") " + target_orient.ToString() + "output";
                Console.WriteLine(s);
            }
            else if (need_deflect && deflect_orient != Orientation.nowhere && Port[(int)deflect_orient].Output != null)
            {
                Port[(int)deflect_orient].Output = mess;
                mess.cnt++;

                string s = "Message " + mess.Id.ToString() + " deflected to Node (" + Location.x.ToString() + " " + Location.y.ToString() + ") " + deflect_orient.ToString() + "output";
                Console.WriteLine(s);
            }
            else if (absorbed)
            {
                string s = "Message " + mess.Id.ToString() + " absorbed" + " life: " + mess.cnt.ToString() + " distance: " + mess.dis.ToString();
                Console.WriteLine(s);

                Stat.dropped[mess.Id] = 0;
                Stat.life[mess.Id] = mess.cnt;
                Stat.distance[mess.Id] = mess.dis;
            }
            else
            {
                string s = "Message " + mess.Id.ToString() + " cannot route" + offset_x.ToString() + offset_y.ToString();
                Console.WriteLine(s);
            }

            Port[porti].Input = null;
        }

        public void RouteMessage_Deflect3(Message mess, int porti)
        {
            int offset_x = mess.Destination.x - Location.x;
            int offset_y = mess.Destination.y - Location.y;

            Orientation target_orient = Orientation.nowhere;
            Orientation deflect_orient = Orientation.nowhere;

            Orientation target_orient2 = Orientation.nowhere;
            Orientation deflect_orient2 = Orientation.nowhere;

            Orientation direction = Orientation.nowhere;

            bool canroute = false;
            bool absorbed = false;
            bool need_deflect = false;
            bool dropped = false;

            if (offset_x == 0 && offset_y == 0)
                absorbed = true;

            if (offset_x > 0)
            {
                target_orient = Orientation.East;
                if(westNeighbour != null) deflect_orient2 = Orientation.West;
            }
            else if(offset_x < 0)
            {
                target_orient = Orientation.West;
                if (eastNeighbour != null) deflect_orient2 = Orientation.East;
            }
            else
            {
                if (westNeighbour != null) deflect_orient2 = Orientation.West;
            }

            if (offset_y > 0)
            {
                target_orient2 = Orientation.North;
                if (southNeighbour != null) deflect_orient = Orientation.South;
            }
            else if(offset_y < 0)
            {
                target_orient2 = Orientation.South;
                if (northNeighbour != null) deflect_orient = Orientation.North;
            }
            else
            {
                target_orient2 = Orientation.North;
            }         

            if(target_orient == Orientation.nowhere || Port[(int)target_orient].Output != null)
            {
                if (target_orient2 == Orientation.nowhere || Port[(int)target_orient2].Output != null)
                {
                     need_deflect = true;
                }
                else
                {
                    canroute = true;
                    direction = target_orient2;
                }
            }
            else
            {
                canroute = true;
                direction = target_orient;
            }

            //=========================

            if(need_deflect)
            {
                if (deflect_orient == Orientation.nowhere || Port[(int)deflect_orient].Output != null)
                {
                    if (deflect_orient2 == Orientation.nowhere)
                    {
                        dropped = true;
                    }
                    else if(Port[(int)deflect_orient2].Output != null)
                    {
                        dropped = true;
                    }
                    else
                    {
                        direction = deflect_orient2;
                    }
                }
                else {
                    direction = deflect_orient;
                }
            }

            //int jk = 0;
            if(need_deflect && direction == Orientation.nowhere)
            {
                dropped = true;

                // Debug.Assert(direction != Orientation.nowhere);
            }

            //=============================

            if(absorbed)
            {
                string s = "Message " + mess.Id.ToString() + " absorbed" + " life: " + mess.cnt.ToString() + " distance: " + mess.dis.ToString();
                Console.WriteLine(s);

                // Stat.dropped[mess.Id] = 0;
                Stat.absorbed[mess.Id] = 1;
                Stat.life[mess.Id] = mess.cnt;
                Stat.distance[mess.Id] = mess.dis;
            }
            else if(dropped)
            {
                string s = "Message " + mess.Id.ToString() + "========================================================================================= dropped at Node (" + Location.x.ToString() + " " + Location.y.ToString() + ")";
                Console.WriteLine(s);

                Stat.dropped[mess.Id] = 1;
                Stat.droploc.Add(Location);
            }
            else if (!need_deflect)
            {                
                Debug.Assert(Port[(int)direction].Output == null);
                Debug.Assert(direction != Orientation.nowhere);


                Port[(int)direction].Output = mess;
                mess.cnt++;

                string s = "Message " + mess.Id.ToString() + " routed to Node (" + Location.x.ToString() + " " + Location.y.ToString() + ") " + direction.ToString() + "output";
                Console.WriteLine(s);
            }
            else
            {               
                Port[(int)direction].Output = mess;
                mess.cnt++;

                string s = "Message " + mess.Id.ToString() + " deflected to Node (" + Location.x.ToString() + " " + Location.y.ToString() + ") " + direction.ToString() + "output";
                Console.WriteLine(s);

                Debug.Assert(direction != Orientation.nowhere);
            }

            Port[porti].Input = null;
        }

        public Orientation DeflectY(int ofst_y)
        {
            Orientation deflect_orient = Orientation.nowhere;

            if (ofst_y > 0)
            {
                deflect_orient = Orientation.North;
            }
            else if (ofst_y < 0)
            {
                deflect_orient = Orientation.South;
            }
            else if(northNeighbour != null)
            {
                deflect_orient = Orientation.North;
            }
            else if(southNeighbour != null)
            {
                deflect_orient = Orientation.South;
            }

            return deflect_orient;
        }

        public Orientation DeflectX(int ofst_x)
        {
            Orientation deflect_orient = Orientation.nowhere;

            if (ofst_x > 0)
            {
                deflect_orient = Orientation.East;
            }
            else if (ofst_x < 0)
            {
                deflect_orient = Orientation.West;
            }
            else if (eastNeighbour != null)
            {
                deflect_orient = Orientation.East;
            }
            else if (westNeighbour != null)
            {
                deflect_orient = Orientation.West;
            }

            return deflect_orient;
        }

        public void UpdateInputs()
        {
            if (westNeighbour != null)
            {
                if (westNeighbour.Port[(int)Orientation.East].Output != null)
                {
                    AddInputMessage(westNeighbour.Port[(int)Orientation.East].Output, Orientation.West);
                    westNeighbour.RemoveOutputMessage(Orientation.East);
                }
            }

            if (southNeighbour != null)
            {
                if (southNeighbour.Port[(int)Orientation.North].Output != null)
                {
                    AddInputMessage(southNeighbour.Port[(int)Orientation.North].Output, Orientation.South);
                    southNeighbour.RemoveOutputMessage(Orientation.North);
                }
            }

            if (eastNeighbour != null)
            {
                if (eastNeighbour.Port[(int)Orientation.West].Output != null)
                {
                    AddInputMessage(eastNeighbour.Port[(int)Orientation.West].Output, Orientation.East);
                    eastNeighbour.RemoveOutputMessage(Orientation.West);
                }
            }

            if (northNeighbour != null)
            {
                if (northNeighbour.Port[(int)Orientation.South].Output != null)
                {
                    AddInputMessage(northNeighbour.Port[(int)Orientation.South].Output, Orientation.North);
                    northNeighbour.RemoveOutputMessage(Orientation.South);
                }
            }
        }
    };

    public class NOC
    {
        public NodeType[,] Node;

        public const int Dim = 8;

        public NOC()
        {
            Node = new NodeType[Dim, Dim];

            for (int i = 0; i < Dim; i++)
            {
                for (int j = 0; j < Dim; j++)
                {
                    Node[i, j] = new NodeType(i + Dim * j, i, j);
                }
            }
        }

        public void InjectTraffic()
        {
            Message Msg = new Message(11);
            Msg.Destination = new Coordinate(3, 3);

            Message Msg2 = new Message(12);
            Msg2.Destination = new Coordinate(3, 3);

            Node[0, 1].AddInputMessage(Msg, Orientation.West);
            Node[1, 0].AddInputMessage(Msg2, Orientation.West);

            Message Msg3 = new Message(13);
            Msg3.Destination = new Coordinate(2, 3);

            Message Msg4 = new Message(14);
            Msg4.Destination = new Coordinate(3, 2);

            Node[0, 2].AddInputMessage(Msg3, Orientation.West);
            Node[2, 0].AddInputMessage(Msg4, Orientation.West);
        }

        public void InjectTraffic(int wave)
        {
            Message Msg = new Message(wave);
            Msg.Destination = new Coordinate(6, 6);

            Message Msg2 = new Message(wave + 1);
            Msg2.Destination = new Coordinate(5, 5);

            Node[0, 1].AddInputMessage(Msg, Orientation.West);
            Node[1, 0].AddInputMessage(Msg2, Orientation.West);

            Message Msg3 = new Message(wave + 2);
            Msg3.Destination = new Coordinate(7, 3);

            Message Msg4 = new Message(wave + 3);
            Msg4.Destination = new Coordinate(3, 5);

            Node[0, 2].AddInputMessage(Msg3, Orientation.West);
            Node[2, 0].AddInputMessage(Msg4, Orientation.West);
        }

        public void InjectTrafficR(int wave)
        {
            Random rnd = new Random();

            int dx, dy;

            dx = rnd.Next(NOC.Dim);
            dy = rnd.Next(NOC.Dim);

            Message Msg = new Message(wave);
            Msg.Destination = new Coordinate(dx, dy);

            dx = rnd.Next(NOC.Dim);
            dy = rnd.Next(NOC.Dim);

            Message Msg2 = new Message(wave + 1);
            Msg2.Destination = new Coordinate(dx, dy);

            Node[0, 0].AddInputMessage(Msg, Orientation.West);
            Node[1, 0].AddInputMessage(Msg2, Orientation.West);

            dx = rnd.Next(NOC.Dim);
            dy = rnd.Next(NOC.Dim);

            Message Msg3 = new Message(wave + 2);
            Msg3.Destination = new Coordinate(dx, dy);

            dx = rnd.Next(NOC.Dim);
            dy = rnd.Next(NOC.Dim);

            Message Msg4 = new Message(wave + 3);
            Msg4.Destination = new Coordinate(dx, dy);

            Node[2, 0].AddInputMessage(Msg3, Orientation.West);
            Node[3, 0].AddInputMessage(Msg4, Orientation.West);
        }

        public void InjectTrafficR2(int wave)
        {
            Random rnd = new Random();

            int dx, dy;

            dx = rnd.Next(NOC.Dim);
            dy = rnd.Next(NOC.Dim);

            Message Msg = new Message(wave + 4);
            Msg.Destination = new Coordinate(dx, dy);

            dx = rnd.Next(NOC.Dim);
            dy = rnd.Next(NOC.Dim);

            Message Msg2 = new Message(wave + 5);
            Msg2.Destination = new Coordinate(dx, dy);

            Node[4, 0].AddInputMessage(Msg, Orientation.West);
            Node[5, 0].AddInputMessage(Msg2, Orientation.West);

            dx = rnd.Next(NOC.Dim);
            dy = rnd.Next(NOC.Dim);

            Message Msg3 = new Message(wave + 6);
            Msg3.Destination = new Coordinate(dx, dy);

            dx = rnd.Next(NOC.Dim);
            dy = rnd.Next(NOC.Dim);

            Message Msg4 = new Message(wave + 7);
            Msg4.Destination = new Coordinate(dx, dy);

            Node[6, 0].AddInputMessage(Msg3, Orientation.West);
            Node[7, 0].AddInputMessage(Msg4, Orientation.West);
        }

        public void InjectTrafficInd(Random rnd, int id, int nx, int ny, Orientation direction)
        {
            int dx, dy;

            dx = rnd.Next(NOC.Dim);
            dy = rnd.Next(NOC.Dim);

            Message Msg = new Message(id);
            Msg.Destination = new Coordinate(dx, dy);

            Node[nx, ny].AddInputMessage(Msg, direction);

            Stat.mess_injected += 1;
            Stat.injected[id] = 1;
        }

        public void InjectTraffic16(Random rnd, int wave)
        {
            int i;
            int id = -1;

            for (i = 0; i < 8; i++)
            {
                id = wave * 16 + i;
                InjectTrafficInd(rnd, id, 0, i, Orientation.West);
            }

            for (i = 0; i < 8; i++)
            {
                id = wave * 16 + 8 + i;
                InjectTrafficInd(rnd, id, 7, i, Orientation.East);
            }
        }

        public void UpdateRouting()
        {
            for(int i=0; i< Dim; i++)
            {
                for(int j=0; j< Dim; j++)
                {
                    Node[i, j].Route();
                }
            }
        }

        public void UpdateInputs()
        {
            for (int i = 0; i < Dim; i++)
            {
                for (int j = 0; j < Dim; j++)
                {
                    Node[i, j].UpdateInputs();
                }
            }
        }

        public void BuildMesh()
        {
            for (int i = 0; i < Dim; i++)
            {
                for (int j = 0; j < Dim; j++)
                {
                    AddNeighbours(i, j);
                }
            }
        }

        public void AddNeighbours(int i, int j)
        {
            if(i > 0)
            {
                Node[i, j].AddNeighbour(ref Node[i - 1, j], Orientation.West);
            }

            if(i < Dim - 1)
            {
                Node[i, j].AddNeighbour(ref Node[i + 1, j], Orientation.East);
            }

            if (j > 0)
            {
                Node[i, j].AddNeighbour(ref Node[i, j - 1], Orientation.South);
            }

            if (j < Dim - 1)
            {
                Node[i, j].AddNeighbour(ref Node[i, j + 1], Orientation.North);
            }
        }
    };



    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello, World! This is NocSim");

            Stat.Init();

            NOC myNoc = new NOC();

            myNoc.BuildMesh();
            // myNoc.InjectTraffic();

            Random rnd = new Random(1);     // seed the rnd to make it repeatable

            for (int i=0; i< 80; i++)
            {
                if (i < 30)
                {
                    //myNoc.InjectTrafficR(i * 8);
                    //myNoc.InjectTrafficR2(i * 8);

                    myNoc.InjectTraffic16(rnd, i);
                }

                myNoc.UpdateRouting();
                myNoc.UpdateInputs();

                String s;

                //if (myNoc.Node[0, 0].Port[(int)Orientation.West].Input != null)
                //    s = myNoc.Node[0, 0].Port[(int)Orientation.West].Input.Id.ToString();
                //else

                    s = "== cycle " + i.ToString();

                //Console.WriteLine(s);
            }

            int x = Stat.dropped[0];

            string csv="";

            for(int j=0; j<480; j++)
            {
                csv += Stat.dropped[j].ToString() + ", " + Stat.life[j].ToString() + ", " + Stat.distance[j].ToString() + ", \n";
            }

            System.IO.File.WriteAllText("rpt.csv", csv, System.Text.Encoding.UTF8);

            Stat.Calc();

            Console.Read();
        }
    }
}
