using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using System.IO;
using Newtonsoft.Json;

namespace MathDash
{
    public class FileInput : Singleton<FileInput>
    {
        public string fName = "test.txt";

        public string filePrefix = "";

        public bool WriteFile(string output)
        {
            return WriteFile(output, fName);
        }

        public bool WriteFile(string output, string fileName)
        {
            try
            {
                string file = filePrefix + fileName;
                //Debug.Log("writing" + output + " to " + file);
                StreamWriter stream = new StreamWriter(file);
                using (stream)
                {
                    stream.Write(output);
                    stream.Close();
                }
                return true;
            }
            catch (Exception ex)
            {
                Debug.Log("exception!" + ex.InnerException);
                return false;
            }
        }

        public string ReadFile(string fileName)
        {
            try
            {
                string output = "";
                string file = filePrefix + fileName;
                StreamReader stream = new StreamReader(file);
                using (stream)
                {
                    output = stream.ReadToEnd();
                    stream.Close();
                }
                //Debug.Log("read in from file "+file + " this " + output);
                return output;
            }
            catch (Exception ex)
            {
                Debug.Log("exception!" + ex.InnerException + " likely file missing at " + fileName);
                return "";
            }
        }

        public string ReadFile()
        {
            return ReadFile(fName);
        }

        public IEnumerable<T> ReadAndDeserialize<T>(string fileName)
        {
            string json = ReadFile(fileName);
            List<T> items = new List<T>();
            //ReadFile returns "" if errors, in that case return empty list
            if(json!="")
            {
                //we read successfully
                 items = JsonConvert.DeserializeObject<List<T>>(json);
            }
            return items;
        }

        public IEnumerable<T> ReadAndDeserialize<T>()
        {
            return ReadAndDeserialize<T>(fName);
        }

        public void WriteAndSerialize<T>(IEnumerable<T> items,string fileName)
        {
            string json = JsonConvert.SerializeObject(items);
            WriteFile(json,fileName);
        }

        public void WriteAndSerialize<T>(IEnumerable<T> items)
        {
            WriteAndSerialize<T>(items, fName);
        }

        public void SetReadFile(string fName)
        {
            this.fName = fName;
        }

        public override void Awake()
        {
            //Debug.Log("when awaek?");
            //if not assigned in inspector, here's a default value
            if(filePrefix=="" || filePrefix ==null)
            {
                filePrefix = Application.persistentDataPath + "/";
            }
            Debug.Log("input/output files are located at: " + filePrefix);
            base.Awake();
           // base.Start();
         //   TestReadWrite();
        }
    }
}
