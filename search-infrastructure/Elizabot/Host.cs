using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Elizabot
{
    public class Host
    {
        private string host;
        private List<Uri> allow;
        private List<Uri> disallow;
        private HashSet<Uri> visited;

        public Host(string host)
        {
            this.host = host;
            allow = new List<Uri>();
            disallow = new List<Uri>();
            visited = new HashSet<Uri>();
        }

        public void addAllow(string relUrl)
        {
            try
            {
                allow.Add(new Uri(relUrl, UriKind.Relative));
            }
            catch (Exception e)
            {
                Console.WriteLine("Error adding allow " + relUrl + "for host " + host.ToString() + ". " + e.ToString());
            }
        }

        public void addDisallow(string relUrl)
        {
            try
            {
                disallow.Add(new Uri(relUrl, UriKind.Relative));
            }
            catch (Exception e)
            {
                Console.WriteLine("Error adding disallow " + relUrl + "for host " + host.ToString() + ". " + e.ToString());
            }
        }

        //adds the given uri as visited
        public void addVisited(Uri uri)
        {
            try
            {
                visited.Add(new Uri(uri.AbsolutePath, UriKind.Relative));
            }
            catch (Exception e)
            {
                Console.WriteLine("Error adding visited " + uri + "for host " + host.ToString() + ". " + e.ToString());
            }
        }

        //returns if the given uri is allowed based on the robots.txt
        public Boolean isAllowed(Uri uri)
        {
            try
            {
                string rel = uri.AbsolutePath;
                if (disallow.Any(x => rel.StartsWith(x.ToString())))
                {
                    if (allow.Any(x => rel.StartsWith(x.ToString())))
                    {
                        return true;
                    }
                    return false;
                }
                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine(uri + "error. " + e.ToString());
                return false;
            }
        }

        //returns if the given uri has been visited before 
        public Boolean hasVisited(Uri uri)
        {
            return (visited.Any(x => x.ToString() == uri.AbsolutePath));
        }
    }
}
