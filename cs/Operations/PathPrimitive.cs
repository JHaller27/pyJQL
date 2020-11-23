using System;
using System.Linq;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Operations
{
    class PathPrimitive : Primitive
    {
        private static Regex OperationRegex = new Regex(@"^(?<part>(\.\w+)|(\[\d*\]))+$");

        private IDictionary<string, dynamic> Json { get; set; }

        private string[] PathElements { get; set; }

        public PathPrimitive(string arg, IDictionary<string, dynamic> json) : base(arg)
        {
            Json = json;

            IList<string> pathParts = new List<string>();

            MatchCollection matches = OperationRegex.Matches(Arg);
            foreach (Match match in matches)
            {
                GroupCollection groups = match.Groups;
                pathParts.Add(groups["part"].Value);
            }

            PathElements = pathParts.ToArray();
        }

        public override Operation ClonePrimitive(string arg)
        {
            return new PathPrimitive(arg, Json);
        }

        public override bool CanParse(string token)
        {
            return OperationRegex.IsMatch(token);
        }

        private static dynamic Traverse(string[] path, dynamic json)
        {
            dynamic curr = json;

            Queue<string> pathQueue = new Queue<string>(path);

            for(string part; pathQueue.TryDequeue(out part); )
            {
                // Move curr as key
                if (part[0] == '.')
                {
                    string key = part.Substring(1);
                    if ( ! ((IDictionary<string, dynamic>) curr).TryGetValue(key, out curr) )
                    {
                        return null;
                    }
                }

                // Move curr as index
                else if (part[0] == '[' && part[part.Length - 1] == ']')
                {
                    string idxStr = part.Substring(1, part.Length - 2).Trim();

                    // Ambiguous indexing - return array of all elements
                    if (idxStr == "")
                    {
                        IList<dynamic> subParts = new List<dynamic>();

                        foreach (dynamic item in ((dynamic[]) curr))
                        {
                            subParts.Add(Traverse(pathQueue.ToArray(), curr));
                        }

                        return subParts.ToArray();
                    }

                    // Specific index
                    int idx = int.Parse(idxStr);
                    curr = ((dynamic[]) curr)[idx];
                }
                else
                {
                    throw new FormatException($"Invalid property-path format '{part}'");
                }
            }

            return curr;
        }

        // TODO Evalute (use Traverse(), then try to convert)

        internal override string EvaluateAsString()
        {
            throw new InvalidOperationException("Operation cannot be evaluated as a string");
        }

        internal override int EvaluateAsInt()
        {
            throw new InvalidOperationException("Operation cannot be evaluated as an integer");
        }

        internal override double EvaluateAsDouble()
        {
            throw new InvalidOperationException("Operation cannot be evaluated as a double");
        }

        internal override bool EvaluateAsBool()
        {
            throw new InvalidOperationException("Operation cannot be evaluated as a boolean");
        }

        internal override dynamic[] EvaluateAsArray()
        {
            throw new InvalidOperationException("Operation cannot be evaluated as an array");
        }

        internal override IDictionary<string, dynamic> EvaluateAsObject()
        {
            throw new InvalidOperationException("Operation cannot be evaluated as an object");
        }
    }
}
