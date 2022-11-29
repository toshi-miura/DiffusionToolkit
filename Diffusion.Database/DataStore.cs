﻿using SQLite;

namespace Diffusion.Database;

public class DataStore
{
    public string DatabasePath { get; }
    public bool RescanRequired { get; set; }

    private SQLiteConnection OpenConnection()
    {
        return new SQLiteConnection(DatabasePath);
    }

    public DataStore(string databasePath)
    {
        DatabasePath = databasePath;
        Create();
    }

    public void Create()
    {
        var databaseDir = Path.GetDirectoryName(DatabasePath);

        if (!Directory.Exists(databaseDir))
        {
            Directory.CreateDirectory(databaseDir);
        }

        using var db = OpenConnection();

        //if (!db.TableExists<Image>())
        //{
        //    //if (db.TableExists<File>())
        //    //{
        //    //    var fileCount = db.ExecuteScalar<int>("select count(*) from File");
        //    //    if (fileCount > 0)
        //    //    {
        //    //        RescanRequired = true;
        //    //    }
        //    //}

        //    //db.DropTable<File>();
        //}

        db.CreateTable<Folder>();
        //db.CreateTable<File>();
        db.CreateTable<Image>();

        db.CreateIndex<Image>(image => image.Path);
        db.CreateIndex<Image>(image => image.ModelHash);
        db.CreateIndex<Image>(image => image.Seed);
        db.CreateIndex<Image>(image => image.Sampler);
        db.CreateIndex<Image>(image => image.Height);
        db.CreateIndex<Image>(image => image.Width);
        db.CreateIndex<Image>(image => image.CFGScale);
        db.CreateIndex<Image>(image => image.Steps);

        db.Close();

    }

    public void AddImage(Image image)
    {
        using var db = OpenConnection();
        db.Insert(image);
        db.Close();
    }

    public void RebuildIndexes()
    {
        using var db = OpenConnection();

        db.DropIndex<Image>(image => image.Path);
        db.DropIndex<Image>(image => image.ModelHash);
        db.DropIndex<Image>(image => image.Seed);
        db.DropIndex<Image>(image => image.Sampler);
        db.DropIndex<Image>(image => image.Height);
        db.DropIndex<Image>(image => image.Width);
        db.DropIndex<Image>(image => image.CFGScale);
        db.DropIndex<Image>(image => image.Steps);

        db.CreateIndex<Image>(image => image.Path);
        db.CreateIndex<Image>(image => image.ModelHash);
        db.CreateIndex<Image>(image => image.Seed);
        db.CreateIndex<Image>(image => image.Sampler);
        db.CreateIndex<Image>(image => image.Height);
        db.CreateIndex<Image>(image => image.Width);
        db.CreateIndex<Image>(image => image.CFGScale);
        db.CreateIndex<Image>(image => image.Steps);
        db.Close();

    }

    public int GetTotal()
    {
        using var db = OpenConnection();

        var count = db.ExecuteScalar<int>("SELECT COUNT(*) FROM Image");

        db.Close();

        return count;
    }

    public IEnumerable<Image> Search(string prompt)
    {
        using var db = OpenConnection();

        var images = db.Query<Image>("SELECT * FROM Image WHERE Prompt LIKE ?", $"%{prompt}%");

        foreach (var image in images)
        {
            yield return image;
        }

        db.Close();
    }

    public bool FolderExists(string path)
    {
        string sql = @"select count(*) from Folder where Path = @path";

        using var db = OpenConnection();
        var cmd = db.CreateCommand("");
        cmd.CommandText = sql;
        cmd.Bind("@path", path);
        var count = cmd.ExecuteScalar<int>();
        db.Close();
        return count > 0;
    }

    public Folder AddFolder(Folder folder)
    {
        string sql = @"insert into Folder (Path) values (@path); select last_insert_rowid();";

        using var db = OpenConnection();
        var cmd = db.CreateCommand("");
        cmd.CommandText = sql;
        cmd.Bind("@path", folder.Path);
        cmd.ExecuteNonQuery();

        sql = "select last_insert_rowid();";

        cmd = db.CreateCommand(sql);
        folder.Id = cmd.ExecuteScalar<int>();

        db.Close();

        return folder;
    }

    //        public void RemoveFolder(int folderId)
    //        {
    //            string sql = @"delete from Folder where Id = @id;";

    //            using var db = OpenConnection();
    //            var cmd = db.CreateCommand("");
    //            cmd.CommandText = sql;
    //            cmd.Bind("@id", folderId);
    //            cmd.ExecuteNonQuery();


    //            db.Close();
    //        }

    //        public void RemoveImages(int folderId)
    //        {
    //            string sql = @"delete from Image where rowid in (select Image.rowid from Image inner join File on Image.FileId = File.Id where FolderId = @id)";

    //            using var db = OpenConnection();
    //            var cmd = db.CreateCommand("");
    //            cmd.CommandText = sql;
    //            cmd.Bind("@id", folderId);
    //            cmd.ExecuteNonQuery();


    //            db.Close();
    //        }

    //        public void RemoveFiles(int folderId)
    //        {
    //            string sql = @"delete from File where FolderId = @id";

    //            using var db = OpenConnection();
    //            var cmd = db.CreateCommand("");
    //            cmd.CommandText = sql;
    //            cmd.Bind("@id", folderId);
    //            cmd.ExecuteNonQuery();


    //            db.Close();
    //        }

    //        public void RemoveInvalidImages(int folderId)
    //        {
    //            string sql = @"delete from Image where rowid in (select Image.rowid from Image inner join File on Image.FileId = File.Id where FolderId = @id) and CheckedState = -1";

    //            using var db = OpenConnection();
    //            var cmd = db.CreateCommand("");
    //            cmd.CommandText = sql;
    //            cmd.Bind("@id", folderId);
    //            cmd.ExecuteNonQuery();


    //            db.Close();
    //        }


    //        public void RemoveUncheckedImages(int folderId)
    //        {
    //            string sql = @"delete from Image (select Image.rowid from Image inner join File on Image.FileId = File.Id where FolderId = @id) and CheckedState in (-1, 0)";

    //            using var db = OpenConnection();
    //            var cmd = db.CreateCommand("");
    //            cmd.CommandText = sql;
    //            cmd.Bind("@id", folderId);
    //            cmd.ExecuteNonQuery();


    //            db.Close();
    //        }

    //        public File AddFile(File file)
    //        {
    //            string sql = @"insert into 
    //File (FolderId, Path, ImageCount) 
    //values (@folderId, @path, @imageCount)";

    //            using var db = OpenConnection();

    //            var cmd = db.CreateCommand("");
    //            cmd.CommandText = sql;
    //            cmd.Bind("@folderId", file.FolderId);
    //            cmd.Bind("@path", file.Path);
    //            cmd.Bind("@imageCount", file.ImageCount);
    //            cmd.ExecuteNonQuery();

    //            sql = "select last_insert_rowid();";

    //            cmd = db.CreateCommand(sql);
    //            file.Id = cmd.ExecuteScalar<int>();

    //            db.Close();

    //            return file;
    //        }

    //        public void AddImage(Image image)
    //        {
    //            using var db = OpenConnection();
    //            db.Insert(image);
    //            db.Close();
    //        }

    //        public void RebuildIndexes()
    //        {
    //            using var db = OpenConnection();
    //            db.DropIndex<File>(file => file.FolderId);

    //            db.DropIndex<Image>(image => image.FileId);
    //            db.DropIndex<Image>(image => image.RecognizedDigits);
    //            db.DropIndex<Image>(image => image.CreatedDate);

    //            db.CreateIndex<File>(file => file.FolderId);
    //            db.CreateIndex<Image>(image => image.FileId);
    //            db.CreateIndex<Image>(image => image.RecognizedDigits);
    //            db.CreateIndex<Image>(image => image.CreatedDate);
    //            db.Close();

    //        }


    //        public IEnumerable<File> GetFiles(int folderId)
    //        {
    //            using var db = OpenConnection();

    //            var files = db.Query<File>("select * from File where FolderId = ?", folderId);

    //            foreach (var file in files)
    //            {
    //                yield return file;
    //            }

    //            db.Close();
    //        }

    //        public IEnumerable<Image> GetFolderImages(int folderId)
    //        {
    //            using var db = OpenConnection();

    //            var images = db.Query<Image>("select Image.* from Image inner join File on Image.FileId = File.Id where File.FolderId = ?", folderId);

    //            foreach (var image in images)
    //            {
    //                yield return image;
    //            }

    //            db.Close();
    //        }

    //        public IEnumerable<Image> GetImages(int fileId)
    //        {
    //            using var db = OpenConnection();

    //            var images = db.Query<Image>("select * from Image where FileId = ?", fileId);

    //            foreach (var image in images)
    //            {
    //                yield return image;
    //            }

    //            db.Close();
    //        }

    //        /// <summary>
    //        /// Sets the checked state of the image (-1=invalid, 0=unchecked, 1=valid)
    //        /// </summary>
    //        /// <param name="imageId"></param>
    //        /// <param name="state"></param>
    //        public void SetCheckedState(int imageId, int state)
    //        {
    //            using var db = OpenConnection();

    //            db.Execute("update Image set CheckedState = ? WHERE Id = ?", state, imageId);

    //            db.Close();
    //        }


    //        /// <summary>
    //        /// Updates the RecognizedDigits of a File and sets IsCorrected to 1
    //        /// </summary>
    //        /// <param name="fileId"></param>
    //        /// <param name="digits"></param>
    //        public void UpdateImage(int fileId, string digits)
    //        {
    //            using var db = OpenConnection();

    //            db.Execute("update Image set RecognizedDigits = ?, IsCorrected = 1 WHERE Id = ?", digits, fileId);

    //            db.Close();
    //        }

    //        public void GetFile(int fileId)
    //        {
    //            throw new NotImplementedException();
    //        }

    //        public void UpdateImage(Image image)
    //        {
    //            using var db = OpenConnection();

    //            db.Update(image);

    //            db.Close();
    //        }


    //        public IEnumerable<PhysicalFile> GetFilePaths(int folderId)
    //        {
    //            using var db = OpenConnection();

    //            var files = db.Query<File>("select FolderId, Path from File where FolderId = ?", folderId);

    //            foreach (var file in files)
    //            {
    //                yield return new PhysicalFile()
    //                {
    //                    FolderId = folderId,
    //                    Path = file.Path
    //                };
    //            }


    //            db.Close();
    //        }

    //        public IEnumerable<FolderView> GetFoldersForView()
    //        {
    //            using var db = OpenConnection();

    //            var folders = db.Query<FolderView>("select Id, Path, (select count(*) from Image inner join File on Image.FileId = File.Id where File.FolderId = Folder.Id) as FileCount from Folder");

    //            foreach (var folder in folders)
    //            {
    //                yield return folder;
    //            }

    //            db.Close();
    //        }

    //        public IEnumerable<Folder> GetFolders()
    //        {
    //            using var db = OpenConnection();

    //            var folders = db.Query<Folder>("select Id, Path from Folder");

    //            foreach (var folder in folders)
    //            {
    //                yield return folder;
    //            }

    //            db.Close();
    //        }


    //        public FolderView GetFolder(int folderId)
    //        {
    //            using var db = OpenConnection();
    //            var result = db.FindWithQuery<FolderView>("select Id, Path from Folder where Id = ?", folderId);
    //            db.Close();
    //            return result;
    //        }

    //        public bool PathExists(string path)
    //        {
    //            using var db = OpenConnection();

    //            var count = db.ExecuteScalar<int>("select Count(Path) from Folder where Path = substr(?, 0, length(Path) + 1)", path);

    //            db.Close();

    //            return count > 0;
    //        }

    //        public File? GetFile(int folderId, string relativePath)
    //        {
    //            using var db = OpenConnection();

    //            var file = db.FindWithQuery<File>("select * from File where FolderId = ? and Path = ?", folderId, relativePath);

    //            db.Close();

    //            return file;
    //        }

    //        private class FileFolderPath
    //        {
    //            public string Folder { get; set; }
    //            public string File { get; set; }
    //        }

    //        public string GetFilePath(int fileId)
    //        {
    //            using var db = OpenConnection();

    //            var ff = db.FindWithQuery<FileFolderPath>("select Folder.Path as Folder, File.Path as File from File inner join Folder on File.FolderId = Folder.Id where File.Id = ?", fileId);

    //            db.Close();

    //            // TODO: find out why this causes an Object reference not set

    //            return Path.Join(ff.Folder, ff.File);
    //        }

    public IEnumerable<string> GetImagePaths()
    {
        List<string> paths = new List<string>();

        using var db = OpenConnection();

        var images = db.Query<Image>("SELECT Path FROM Image");

        foreach (var image in images)
        {
            paths.Add(image.Path);
            //yield return image.Path;
        }

        db.Close();

        return paths;
    }
}