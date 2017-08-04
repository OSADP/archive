/*
 * To change this template, choose Tools | Templates
 * and open the template in the editor.
 */
package org.noblis;

import java.io.File;
import java.io.IOException;
import java.text.DateFormat;
import java.text.DecimalFormat;
import java.text.SimpleDateFormat;
import java.util.Calendar;
import java.util.Date;
import java.util.Iterator;
import java.util.List;
import java.util.logging.Level;
import java.util.logging.Logger;
import javax.servlet.ServletContext;
import javax.servlet.ServletException;
import javax.servlet.annotation.WebServlet;
import javax.servlet.http.HttpServlet;
import javax.servlet.http.HttpServletRequest;
import javax.servlet.http.HttpServletResponse;
import javax.servlet.http.HttpSession;
import org.apache.commons.fileupload.FileItem;
import org.apache.commons.fileupload.FileUploadException;
import org.apache.commons.fileupload.ProgressListener;
import org.apache.commons.fileupload.disk.DiskFileItemFactory;
import org.apache.commons.fileupload.servlet.FileCleanerCleanup;
import org.apache.commons.fileupload.servlet.ServletFileUpload;
import org.apache.commons.io.FileCleaningTracker;
import org.apache.commons.io.FileUtils;
import org.apache.commons.io.FilenameUtils;
import org.json.JSONException;
import org.json.JSONObject;

/**
 *
 * @author m29605
 */
@WebServlet(name = "FileUpload", urlPatterns = {"/FileUpload"})
public class FileUpload extends HttpServlet {

    private final File repository = new File("/data/copilot/");

    /**
     * Processes requests for both HTTP <code>GET</code> and <code>POST</code>
     * methods.
     *
     * @param request servlet request
     * @param response servlet response
     * @throws ServletException if a servlet-specific error occurs
     * @throws IOException if an I/O error occurs
     */
    protected void processRequest(HttpServletRequest request, HttpServletResponse response)
            throws ServletException, IOException {
        //System.out.println(request.toString());
        String action = request.getParameter("action");
        if (action == null) {
            processDefault(request, response);
        } else if (action.equalsIgnoreCase("upload")) {
            processUpload(request, response);
        } else {
            processDefault(request, response);
        }
    }

    // <editor-fold defaultstate="collapsed" desc="HttpServlet methods. Click on the + sign on the left to edit the code.">
    /**
     * Handles the HTTP <code>GET</code> method.
     *
     * @param request servlet request
     * @param response servlet response
     * @throws ServletException if a servlet-specific error occurs
     * @throws IOException if an I/O error occurs
     */
    @Override
    protected void doGet(HttpServletRequest request, HttpServletResponse response)
            throws ServletException, IOException {
        processRequest(request, response);
    }

    /**
     * Handles the HTTP <code>POST</code> method.
     *
     * @param request servlet request
     * @param response servlet response
     * @throws ServletException if a servlet-specific error occurs
     * @throws IOException if an I/O error occurs
     */
    @Override
    protected void doPost(HttpServletRequest request, HttpServletResponse response)
            throws ServletException, IOException {
        processRequest(request, response);
    }

    /**
     * Returns a short description of the servlet.
     *
     * @return a String containing servlet description
     */
    @Override
    public String getServletInfo() {
        return "Short description";
    }
    // </editor-fold>

    protected void processUpload(HttpServletRequest request, HttpServletResponse response) {
        // Create a factory for disk-based file items
        DiskFileItemFactory factory = newDiskFileItemFactory(getServletContext(), repository);

        // Set factory constraints
        // factory.setSizeThreshold(yourMaxMemorySize);
        // factory.setRepository(yourTempDirectory);
        // Create a new file upload handler
        ServletFileUpload upload = new ServletFileUpload(factory);

        // Set overall request size constraint
        // upload.setSizeMax(yourMaxRequestSize);
        //Create a progress listener
        ProgressListener progressListener = new ProgressListener() {
            private JSONObject objProgress = new JSONObject();
            private long megaBytes = -1;
            private final DecimalFormat df = new DecimalFormat("#.##");

            @Override
            public void update(long pBytesRead, long pContentLength, int pItems) {
                if (pBytesRead == pContentLength) {
                    setProgress(pBytesRead, pContentLength, pItems);
                    return;
                }

                long mBytes = pBytesRead / 1000000;
                if (megaBytes == mBytes) {
                    return;
                }
                megaBytes = mBytes;

                setProgress(pBytesRead, pContentLength, pItems);
            }

            public void setProgress(long pBytesRead, long pContentLength, int pItems) {
                try {
                    double percentComplete = ((double) pBytesRead / (double) pContentLength) * 100;
                    objProgress.put("pBytesRead", pBytesRead);
                    objProgress.put("pContentLength", pContentLength);
                    objProgress.put("pItems", pItems);
                    objProgress.put("pCompleted", df.format(percentComplete));
                } catch (JSONException ex) {
                    Logger.getLogger(FileUpload.class.getName()).log(Level.SEVERE, null, ex);
                }
            }

            @Override
            public String toString() {
                try {
                    return objProgress.toString(4);
                } catch (JSONException ex) {
                    Logger.getLogger(FileUpload.class.getName()).log(Level.SEVERE, null, ex);
                }
                return "{}";
            }
        };
        upload.setProgressListener(progressListener);

        HttpSession session = request.getSession();
        session.setAttribute("progressListener", progressListener);

        try {
            // Parse the request
            List<FileItem> items = upload.parseRequest(request);
            // Process the uploaded items
            Iterator<FileItem> iter = items.iterator();
            while (iter.hasNext()) {
                FileItem item = iter.next();
                String rand = request.getParameter("rand");
                if (item.isFormField()) {
                    System.out.println("FileUpload: " + getDateString() + ": processFormField");
                    processFormField(item);
                } else {
                    System.out.println("FileUpload: " + getDateString() + ": processUploadedFile");
                    processUploadedFile(item, rand);
                }
            }
        } catch (FileUploadException ex) {
            Logger.getLogger(FileUpload.class.getName()).log(Level.SEVERE, null, ex);
            upload.getProgressListener().update(-1, -1, -1);
        }
    }

    public static String getDateString() {
        DateFormat df = new SimpleDateFormat("yyyy/MM/dd HH:mm:ss");
        Date today = Calendar.getInstance().getTime();
        return df.format(today);
    }

    public static String getDateString(Date d) {
        DateFormat df = new SimpleDateFormat("yyyy/MM/dd HH:mm:ss");
        return df.format(d);
    }

    public static String getDateString(long time) {
        DateFormat df = new SimpleDateFormat("yyyy/MM/dd HH:mm:ss");
        return df.format(new Date(time));
    }

    private void processFormField(FileItem item) {
        String name = item.getFieldName();
        String value = item.getString();
        System.out.println("FileUpload: " + getDateString() + ": " + name);
        System.out.println("FileUpload: " + getDateString() + ": " + value);
    }

    private void processUploadedFile(FileItem item, String rand) {
        //String fieldName = item.getFieldName();
        //String fileName = item.getName();
        //String contentType = item.getContentType();
        //boolean isInMemory = item.isInMemory();
        //long sizeInBytes = item.getSize();

        String fileName = FilenameUtils.getName(item.getName().replaceAll(" +", " ")).replaceAll("#", "");
        String path = "";
        path += repository.getPath();
        path += System.getProperty("file.separator");
        path += rand + "_";
        path += fileName;

        File file = new File(path);
        file.setReadable(true);
        file.setWritable(true);
        if (file.exists()) {
            System.out.println("FileUpload: " + getDateString() + ": about to overwrite and blow away: " + file.getAbsolutePath());
        }
        try {
            item.write(file);
        } catch (Exception ex) {
            Logger.getLogger(FileUpload.class.getName()).log(Level.SEVERE, null, ex);
        }
        /*
         try {
         InputStream filecontent;
         try (OutputStream out = new FileOutputStream(new File(path))) {
         filecontent = item.getInputStream();
         int read = 0;
         final byte[] bytes = new byte[1024];
         while ((read = filecontent.read(bytes)) != -1) {
         out.write(bytes, 0, read);
         }
         System.out.println("New file " + fileName + " created at " + path);
         out.flush();
         }
         filecontent.close();
         } catch (FileNotFoundException fne) {
         System.err.println("FileNotFoundError: " + fne.getMessage());
         } catch (IOException ex) {
         Logger.getLogger(FileUpload.class.getName()).log(Level.SEVERE, null, ex);
         } finally {
         // nothing to do at the moment...
         }
         */
    }

    public static DiskFileItemFactory newDiskFileItemFactory(ServletContext context, File repository) {
        FileCleaningTracker fileCleaningTracker = FileCleanerCleanup.getFileCleaningTracker(context);
        DiskFileItemFactory factory = new DiskFileItemFactory(DiskFileItemFactory.DEFAULT_SIZE_THRESHOLD, repository);
        factory.setFileCleaningTracker(fileCleaningTracker);
        return factory;
    }

    private void processDefault(HttpServletRequest request, HttpServletResponse response) throws IOException {
        response.getOutputStream().println(request.getParameterMap().toString());
    }
}
