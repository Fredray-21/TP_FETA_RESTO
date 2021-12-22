﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;
using MySql.Data;
using System.Data;

namespace TP_FETA_RESTO
{
    public static class ORMmySQL
    {
        public static Compte CurrentUser = null;
        public static int _counterForm = 0; // permet de renomé et supprimer les formulaire voulu
        public static List<Formule> Panier = new List<Formule>();
        // Note
        // ExecuteNonQuery() -> int  uptade/insert/delete 
        // ExecuteReader() -> table  Select
        // ExecuteScalar() -> objet  Si on veux resevoir un object

        private static String cs = @"server=localhost;userid=fetaresto;password=;database=feta_resto";
        private static MySqlConnection conn = null;

        public static bool ConnexionDB()
        {
            conn = new MySqlConnection(cs);
            conn.Open();
            return (conn.State == ConnectionState.Open);
        }

        public static Compte ConnexionCompte(String AdrMail, String MDP)
        {
            MySqlCommand objCmd;
            objCmd = conn.CreateCommand();
            MySqlDataReader rdr;

            String reqCount = $"SELECT * FROM compte WHERE ADRMAILCPTE = '{AdrMail}' AND MDP = '{MDP}'";
            objCmd.CommandText = reqCount;
            rdr = objCmd.ExecuteReader();
            Compte p = null;
            if (rdr.Read())
            {
                p = new Compte((int)rdr["idUser"], (String)rdr["MDP"], (String)rdr["NOMCPTE"], (String)rdr["PRENOMCPTE"], (DateTime)rdr["DATEINSCRIP"], (String)rdr["ADRMAILCPTE"], (String)rdr["NOTELCPTE"], (String)rdr["TYPECOMPTE"]);

            }
            rdr.Close();
            return p;
        }

        public static bool UpdateCompte(String Nom, String Prenom, String AdresseMail, String NoTel)
        {
            Compte currentCompte = ORMmySQL.CurrentUser;
            if (currentCompte == null)
            {
                return false;
            }
            else
            {
                MySqlCommand objCmd;
                objCmd = conn.CreateCommand();
                String reqU = $"UPDATE compte SET NOMCPTE = '{Nom}',PRENOMCPTE = '{Prenom}',ADRMAILCPTE = '{AdresseMail}',NOTELCPTE = '{NoTel}' WHERE idUser = '{currentCompte.GetIdUser().ToString()}' ";
                objCmd.CommandText = reqU;
                int nbMaj = objCmd.ExecuteNonQuery();
                if (nbMaj == 0)
                {
                    return false;
                }
            }
            return true;
        }


        public static List<Formule> GetAllFormules()
        {
            MySqlCommand objCmd;
            objCmd = conn.CreateCommand();
            MySqlDataReader rdr;
            List<Formule> TouteLesFormule = new List<Formule>();

            String reqCount = $"SELECT * FROM formules";
            objCmd.CommandText = reqCount;
            rdr = objCmd.ExecuteReader();
            while (rdr.Read())
            {
                Formule f = new Formule((int)rdr["NOFORMULE"], (String)rdr["NOMFORMULE"], (float)rdr["PRIXFORMULE"]);
                TouteLesFormule.Add(f);
            }
            rdr.Close();
            return TouteLesFormule;
        }

        public static Formule GetFormule(int NOFORMULE)
        {
            MySqlCommand objCmd;
            objCmd = conn.CreateCommand();
            MySqlDataReader rdr;

            String reqCount = $"SELECT * FROM formules WHERE NOFORMULE = {NOFORMULE}";
            objCmd.CommandText = reqCount;
            rdr = objCmd.ExecuteReader();
            Formule f = null;
            if (rdr.Read())
            {
                f = new Formule((int)rdr["NOFORMULE"], (String)rdr["NOMFORMULE"], (float)rdr["PRIXFORMULE"]);
            }
            rdr.Close();
            return f;
        }

        public static bool AjouterArticle(String Nom, String Description, String TypeArticle)
        {
            MySqlCommand objCmd;
            objCmd = conn.CreateCommand();

            String reqI = $"INSERT INTO articles (NOMARTICLE,DESCARTICLE,TYPEARTICLE) VALUES(\"{Nom}\",\"{Description}\",'{TypeArticle}')";
            objCmd.CommandText = reqI;
            int nbMaj = objCmd.ExecuteNonQuery();
            return (nbMaj == 1);

        }

        public static List<Article> GetAllArticles()
        {
            MySqlCommand objCmd;
            objCmd = conn.CreateCommand();
            MySqlDataReader rdr;
            List<Article> ToutLesArticles = new List<Article>();

            String reqCount = $"SELECT * FROM articles";
            objCmd.CommandText = reqCount;
            rdr = objCmd.ExecuteReader();
            while (rdr.Read())
            {
                Article a = new Article((int)rdr["NOARTICLE"], (String)rdr["NOMARTICLE"], (String)rdr["DESCARTICLE"], "", (String)rdr["TYPEARTICLE"]);
                ToutLesArticles.Add(a);
            }
            rdr.Close();
            return ToutLesArticles;
        }


        public static bool AjouterFormule(List<Article> lesArticleSelected, String Nom, float Prix)
        {
            MySqlCommand objCmd;
            objCmd = conn.CreateCommand();
            float NOFORMULE = -1;

            String reqI = $"INSERT INTO formules (NOMFORMULE,PRIXFORMULE) VALUES('{Nom}','{Prix}')";
            objCmd.CommandText = reqI;
            int nbMaj = objCmd.ExecuteNonQuery();
            if (nbMaj == 1)
            {
                String reqNumId = "SELECT LAST_INSERT_ID()";
                objCmd.CommandText = reqNumId;
                object result = objCmd.ExecuteScalar();
                bool convertOK = float.TryParse(result.ToString(), out float id);

                if (convertOK)
                {
                    NOFORMULE = id;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
            //fin ajoute table formule

            objCmd = conn.CreateCommand();
            String reqI2 = $"";
            foreach (Article a in lesArticleSelected)
            {
                reqI2 = reqI2 + $"INSERT INTO contient (NOFORMULE,NOARTICLE) VALUES({NOFORMULE},{a.GetIdArticle()});";
            }
            objCmd.CommandText = reqI2;
            objCmd.ExecuteNonQuery();
            return true;
        }
    }
}
