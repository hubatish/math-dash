using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace MathDash
{
    public class JsonReaderTest : MonoBehaviour
    {
        protected void Start()
        {

        }

        protected void TestFileAndEquation()
        {
            //string str = FileInput.ReadFile();
            Equation first = new Equation
            {
                solution = 3
            };
            Debug.Log(first.ToString());
            FileInput.Instance.SetReadFile("equations");
            FileInput.Instance.WriteAndSerialize(new List<Equation>() { first });
            IEnumerable<Equation> blank = FileInput.Instance.ReadAndDeserialize<Equation>();
            if (blank == null)
            {
                Debug.Log("nothing came back from reading");
            }
            else
            {
                Debug.Log(blank.ToString());
            }
        }

        protected void TestSimpleClass()
        {
            //string str = FileInput.ReadFile();
            SimpleClass first = new SimpleClass
            {
                hello = "world"
            };
            first.Print();
            string str = JsonConvert.SerializeObject(first);
            SimpleClass blank = JsonConvert.DeserializeObject<SimpleClass>(str);
            if (blank == null)
            {
                Debug.Log("nothing came back from " + str);
            }
            else
            {
                blank.Print();
            }
        }
    }

    public class SimpleClass
    {
        public string hello;
        public void Print()
        {
            Debug.Log("hello " + hello);
        }
    }
}
