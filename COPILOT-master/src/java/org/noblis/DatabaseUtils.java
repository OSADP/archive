/*
 * To change this license header, choose License Headers in Project Properties.
 * To change this template file, choose Tools | Templates
 * and open the template in the editor.
 */
package org.noblis;

import java.io.BufferedReader;
import java.io.IOException;
import java.io.InputStream;
import java.io.InputStreamReader;
import java.sql.Connection;
import java.sql.DatabaseMetaData;
import java.sql.Date;
import java.sql.DriverManager;
import java.sql.PreparedStatement;
import java.sql.ResultSet;
import java.sql.ResultSetMetaData;
import java.sql.SQLException;
import java.sql.Timestamp;
import java.util.ArrayList;
import java.util.HashMap;
import java.util.Map;
import java.util.Set;
import java.util.logging.Level;
import java.util.logging.Logger;
import org.json.JSONException;
import org.json.JSONObject;

/**
 *
 * @author M29538
 */
public class DatabaseUtils {

    /**
     * Get a connection to the database with a (@code JSONObject} initializer
     * containing values for the keys: driver, dbms, host, db, user, and pass.
     * On error, return {@code null}.
     *
     * @param objINI the (@code JSONObject} to initialize the connection
     * @param admin
     * @return {@code Connection}
     */
    public Connection getConnection(JSONObject objINI, boolean admin) {
        // <editor-fold defaultstate="collapsed" desc="getConnection">
        // check objINI: if null or is missing keys, then no connection...
        if (objINI == null) {
            return null;
        }
        String driver = objINI.optString("driver");
        String dbms = objINI.optString("dbms");
        String host = objINI.optString("host");
        String db = objINI.optString("db");
        String user = admin ? objINI.optString("adminUser") : objINI.optString("user");
        String pass = admin ? objINI.optString("adminPass") : objINI.optString("pass");
        String url = String.format("%s:%s://%s/%s", driver, dbms, host, db);
        if (dbms.equals("postgresql")) {
            try {
                Class.forName("org.postgresql.Driver");
            } catch (ClassNotFoundException ex) {
                String msg = String.format("Failed to load driver \"%s\"...", driver);
                Logger.getLogger(DatabaseUtils.class.getName()).log(Level.SEVERE, msg, ex);
            }
        }
        Connection conn;
        try {
            conn = DriverManager.getConnection(url, user, pass);
        } catch (SQLException ex) {
            String msg = String.format("Failed to get database connection \"%s\"...", objINI);
            Logger.getLogger(DatabaseUtils.class.getName()).log(Level.SEVERE, msg, ex);
            conn = null; // set it to something that can not be used.
        }

        return conn;
        // </editor-fold>
    }

    /**
     * Short hand for non-admin connection (admin means update/insert)
     *
     * @param path
     * @return
     */
    public Connection getConnection(String path) {
        return getConnection(path, false);
    }

    /**
     * Get a connection to the database with a JSON initializer file containing
     * values for the keys: driver, dbms, host, db, user, and pass. On error,
     * return {@code null}.
     *
     * @param path the path to the JSON initialization file
     * @param admin
     * @return {@code Connection}
     */
    public Connection getConnection(String path, boolean admin) {
        // <editor-fold defaultstate="collapsed" desc="getConnection">
        String strINI = readResourceFile(path);
        JSONObject objINI;
        try {
            objINI = new JSONObject(strINI);
        } catch (JSONException ex) {
            return null;
        }
        return getConnection(objINI, admin);
        // </editor-fold>
    }

    /**
     * Read the JSON file contents into a {@code JSONObject}. On error or null
     * parameter, the returned {@code JSONObject} is empty.
     *
     * @param path the path to the resource
     * @return {@code JSONObject}
     */
    public String readResourceFile(String path) {
        // <editor-fold defaultstate="collapsed" desc="readResourceFile">
        String contents = "";
        try {
            InputStream is = this.getClass().getResourceAsStream(path);
            BufferedReader br = new BufferedReader(new InputStreamReader(is));
            String line;
            while ((line = br.readLine()) != null) {
                contents += line;
            }
        } catch (IOException ex) {
            Logger.getLogger(DatabaseUtils.class.getName()).log(Level.SEVERE, null, ex);
        }
        return contents;
        // </editor-fold>
    }

    /**
     * handle closing the connection to the database including handling the
     * exception
     *
     * @param conn
     * @return Returns true if the connection is closed
     */
    public boolean closeConnection(Connection conn) {
        // <editor-fold defaultstate="collapsed" desc="closeConnection">
        boolean b = false;
        try {
            if (conn != null && conn.isClosed()) {
                b = true;
            } else if (conn != null) {
                conn.close();
                b = conn.isClosed();
            } else {
                b = true; // conn is null; so it must be closed
            }
        } catch (SQLException ex) {
            Logger.getLogger(DatabaseUtils.class.getName()).log(Level.SEVERE, null, ex);
        }
        //System.out.println(String.format("Connection is %s", (b ? "now closed!" : "still open!")));
        return b;
        // </editor-fold>
    }

    /**
     * Return all records (no where clause or order by clause)
     *
     * @param conn
     * @param schema
     * @param table
     * @return
     */
    public ArrayList<HashMap<String, String>> selectAllRecords(Connection conn, String schema, String table) {
        return selectRecords(conn, schema, table, null, null);
    }

    /**
     *
     * @param conn
     * @param schema
     * @param table
     * @param sort
     * @return
     */
    public ArrayList<HashMap<String, String>> selectAllRecords(Connection conn, String schema, String table, ArrayList<HashMap<String, String>> sort) {
        return selectRecords(conn, schema, table, null, sort);
    }

    /**
     * Query the database and return the result set as a list of hashes. Note:
     * this method does not close the connection!
     *
     * @param conn
     * @param schema
     * @param table
     * @param where
     * @return
     */
    public ArrayList<HashMap<String, String>> selectRecords(Connection conn, String schema, String table, HashMap<String, String> where) {
        return selectRecords(conn, schema, table, where, null);
    }

    /**
     * Query the database and return the result set as a list of hashes. Note:
     * this method does not close the connection!
     *
     * @param conn the database connection object
     * @param schema the database schema
     * @param table the database table
     * @param where the values to use in the where clause, can be null or empty
     * @param sort the string that defines the sort (user must build at this
     * point) optional, may be null or blank)
     * @return list of hashes
     */
    public ArrayList<HashMap<String, String>> selectRecords(Connection conn, String schema, String table, HashMap<String, String> where, ArrayList<HashMap<String, String>> sort) {
        // <editor-fold defaultstate="collapsed" desc="selectRecords">

        // not sure if this should be done here... but lets check to see if the schema table combination exists
        if (!tableExists(conn, schema, table)) {
            System.err.println(String.format("Schema->Table (%s.%s) combination is not valid", schema, table));
            return new ArrayList<HashMap<String, String>>();
        }
        HashMap<String, String> metadata = getTableMetadata(conn, schema, table);
        ArrayList<String> params = new ArrayList<String>();
        ArrayList<String> paramType = new ArrayList<String>();

        // build SQL query
        String whereSQL = "";
        if (where != null && !where.isEmpty()) {
            whereSQL = "WHERE ";
            Set<String> keys = where.keySet();
            int i = 1;
            for (String key : keys) {
                String pt = metadata.get(key);
                if (pt == null) { // TODO: look into removing this
                    System.out.println(key);
                    continue;
                }
                params.add(where.get(key));
                paramType.add(pt);
                if (pt.equalsIgnoreCase("varchar") || pt.equalsIgnoreCase("text") || pt.equalsIgnoreCase("bpchar")) {
                    whereSQL += i != 1 ? String.format("AND LOWER(%s) = LOWER(?) ", key) : String.format("LOWER(%s) = LOWER(?) ", key);
                } else {
                    whereSQL += i != 1 ? String.format("AND %s = ? ", key) : String.format("%s = ? ", key);
                }
                i++;
            }
        }
        String order = "";
        if (sort != null && !sort.isEmpty()) {
            order += "ORDER BY ";
            int i = 0;
            for (HashMap<String, String> strs : sort) {
                order += String.format("%s %s", strs.get("column"), strs.get("direction"));
                order += i == sort.size() - 1 ? "" : ", ";
                i++;
            }
        }
        String sql = String.format("SELECT * FROM %s.%s %s %s", schema, table, whereSQL, order);

        return sqlExecution(conn, sql, params, paramType); // use the private one since we controlled the method that it was generated
        // </editor-fold>
    }

    /**
     * NOTE: This does not close the result set!
     *
     * @param rs takes a Result Set and produces a Array List of Hash Maps;
     * removes nulls
     * @return
     * @throws java.sql.SQLException
     */
    public ArrayList<HashMap<String, String>> convertResultSetToArrayList(ResultSet rs) throws SQLException {
        // <editor-fold defaultstate="collapsed" desc="convertResultSetToArrayList">
        ArrayList<HashMap<String, String>> rows = new ArrayList<HashMap<String, String>>();
        // get the number of columns and column names
        ResultSetMetaData rsmd = rs.getMetaData();
        int ncols = rsmd.getColumnCount();
        // generate array list of hash rows
        while (rs.next()) {
            HashMap<String, String> row = new HashMap<String, String>();
            for (int i = 1; i <= ncols; i++) {
                String str = rs.getString(i);
                str = str == null ? "" : str.trim(); //may need to do this for better results....
                row.put(rsmd.getColumnName(i), str);
            }
            rows.add(row);
        }
        return rows;
        // </editor-fold>
    }

    /**
     * Determines if a table exists
     *
     * @param conn
     * @param schema
     * @param table
     * @return
     */
    public boolean tableExists(Connection conn, String schema, String table) {
        // <editor-fold defaultstate="collapsed" desc="tableExists">
        boolean exists = false;
        ResultSet rs = null;
        try {
            DatabaseMetaData metaData = conn.getMetaData();
            rs = metaData.getSchemas();
            ArrayList<HashMap<String, String>> schemas = convertResultSetToArrayList(rs);
            rs = metaData.getTables(null, schema, table, null);
            ArrayList<HashMap<String, String>> tables = convertResultSetToArrayList(rs);

            // check to see if the schema exists
            boolean schemaExists = false;
            for (HashMap<String, String> row : schemas) {
                if (row.get("table_schem").equalsIgnoreCase(schema)) {
                    schemaExists = true;
                }
            }
            if (!schemaExists) {
                exists = false;
            } else {
                for (HashMap<String, String> row : tables) {
                    if (row.get("table_name").equalsIgnoreCase(table)) {
                        exists = true;
                    }
                }
            }
        } catch (SQLException ex) {
            Logger.getLogger(DatabaseUtils.class.getName()).log(Level.SEVERE, null, ex);
        } finally {
            try {
                if (rs != null) {
                    rs.close();
                }
            } catch (SQLException ex) {
                Logger.getLogger(DatabaseUtils.class.getName()).log(Level.SEVERE, null, ex);
            }
        }
        return exists;
        // </editor-fold>
    }

    /**
     *
     * @param conn
     * @param schema
     * @param table
     * @return Returns a hashmap of columns and data types
     */
    private HashMap<String, String> getTableMetadata(Connection conn, String schema, String table) {
        // <editor-fold defaultstate="collapsed" desc="getTableMetadata">
        PreparedStatement ps = null;
        ResultSet rs = null;
        String selectSQL = String.format("SELECT * FROM %s.%s LIMIT 2", schema, table); // don't need alot of data...
        HashMap<String, String> md = new HashMap<String, String>();
        try {
            ps = conn.prepareCall(selectSQL);
            rs = ps.executeQuery();
            ResultSetMetaData metaData = rs.getMetaData();

            int numCols = metaData.getColumnCount();
            for (int i = 1; i <= numCols; i++) {
                md.put(metaData.getColumnName(i), metaData.getColumnTypeName(i));
            }
        } catch (SQLException ex) {
            Logger.getLogger(DatabaseUtils.class.getName()).log(Level.SEVERE, null, ex);
        } finally {
            try {
                if (ps != null) {
                    ps.close();
                }
                if (rs != null) {
                    rs.close();
                }
            } catch (SQLException ex) {
                Logger.getLogger(DatabaseUtils.class.getName()).log(Level.SEVERE, null, ex);
            }
        }
        return md;
        // </editor-fold>
    }

    /**
     *
     * @param metadata
     * @return returns the auto increment column of the table or "" if none
     */
    private String getSerialColumnName(HashMap<String, String> metadata) {
        // <editor-fold defaultstate="collapsed" desc="getSerialColumnName">
        for (Map.Entry<String, String> val : metadata.entrySet()) {
            if (val.getValue().equalsIgnoreCase("serial") || val.getValue().equalsIgnoreCase("bigserial")) {
                return val.getKey();
            }
        }
        return ""; // error case...
        // </editor-fold>
    }

    public String getSerialColumnName(Connection conn, String schema, String table) {
        return getSerialColumnName(getTableMetadata(conn, schema, table));
    }

    /**
     *
     * @param conn
     * @param schema
     * @param table
     * @return Returns a list of the columns that comprise the pk
     */
    private ArrayList<String> getPrimaryKey(Connection conn, String schema, String table) {
        // <editor-fold defaultstate="collapsed" desc="getPrimaryKey">
        ArrayList<String> pk = new ArrayList<String>();
        ResultSet result = null;
        String catalog = null;
        DatabaseMetaData databaseMetaData;
        try {
            databaseMetaData = conn.getMetaData();
            result = databaseMetaData.getPrimaryKeys(catalog, schema, table);
            while (result.next()) {
                pk.add(result.getString(4));
            }
        } catch (SQLException ex) {
            Logger.getLogger(DatabaseUtils.class.getName()).log(Level.SEVERE, null, ex);
        } finally {
            if (result != null) {
                try {
                    result.close();
                } catch (SQLException ex) {
                    Logger.getLogger(DatabaseUtils.class.getName()).log(Level.SEVERE, null, ex);
                }
            }
        }
        return pk;
        // </editor-fold>
    }

    public boolean hasPKColumns(Connection conn, String schema, String table, HashMap<String, String> data) {
        // <editor-fold defaultstate="collapsed" desc="hasPKColumns">
        boolean hasPKColumns = false;
        ArrayList<String> primaryKey = getPrimaryKey(conn, schema, table);
        for (String key : primaryKey) {
            if (data.containsKey(key)) {
                hasPKColumns = true;
            }
        }
        return hasPKColumns;
        // </editor-fold>
    }

    /**
     * Given a table and some data, will determine if that data already
     * exists... useful for checking for pk violations
     *
     * @param conn
     * @param schema
     * @param table
     * @param data
     * @return
     */
    private boolean checkIfExists(Connection conn, String schema, String table, HashMap<String, String> data) {
        // <editor-fold defaultstate="collapsed" desc="checkIfExists">
        boolean b = false;
        HashMap<String, String> metadata = getTableMetadata(conn, schema, table);
        PreparedStatement ps = null;
        ResultSet rs = null;
        String sql = String.format("SELECT * FROM %s.%s WHERE 1=1 ", schema, table);
        ArrayList<String> params = new ArrayList<String>();
        ArrayList<String> paramType = new ArrayList<String>();
        for (String d : data.keySet()) {
            String p = metadata.get(d);
            sql += p.equalsIgnoreCase("bool") || p.equalsIgnoreCase("int4") || p.equalsIgnoreCase("int8") ? String.format("AND %s = ? ", d) : String.format("AND LOWER(%s) = LOWER(?) ", d); // should this use lower case???
            params.add(data.get(d));
            paramType.add(metadata.get(d));
        }
        try {
            ps = conn.prepareStatement(sql);
            int q = 1;
            for (String p : params) {
                String pt = paramType.get(q - 1); // ugh... this one uses 0 based...
                if (pt.equalsIgnoreCase("varchar") || pt.equalsIgnoreCase("text") || pt.equalsIgnoreCase("bpchar")) {
                    ps.setString(q, p);
                } else if (pt.equalsIgnoreCase("bool")) {
                    ps.setBoolean(q, Boolean.parseBoolean(p));
                } else if (pt.equalsIgnoreCase("int8") || pt.equalsIgnoreCase("int4")) {
                    ps.setInt(q, Integer.parseInt(p));
                } else if (pt.equalsIgnoreCase("timestamp")) {
                    ps.setTimestamp(q, Timestamp.valueOf(p));
                } else if (pt.equalsIgnoreCase("date")) {
                    ps.setDate(q, Date.valueOf(p));
                }
                q++;
            }
            rs = ps.executeQuery();
            while (rs.next()) {
                b = true;   // well, if it returns something, it must already be there... right?
            }
        } catch (SQLException ex) {
            Logger.getLogger(DatabaseUtils.class.getName()).log(Level.SEVERE, null, ex);
        } finally {
            try {
                if (ps != null) {
                    ps.close();
                }
                if (rs != null) {
                    rs.close();
                }
            } catch (SQLException ex) {
                Logger.getLogger(DatabaseUtils.class.getName()).log(Level.SEVERE, null, ex);
            }
        }
        return b;
        // </editor-fold>
    }

    /**
     *
     * @param conn
     * @param schema
     * @param table
     * @param data
     * @return Returns true if the primary key is not violated by the data
     * provided; violation could be not all parts of the PK are represented or
     * because it already exists
     */
    public boolean doesNotViolatesPrimaryKey(Connection conn, String schema, String table, HashMap<String, String> data) {
        // <editor-fold defaultstate="collapsed" desc="doesNotViolatesPrimaryKey">
        ArrayList<String> pks = getPrimaryKey(conn, schema, table);
        // first test is to make sure the pk is in the data...
        HashMap<String, String> pkData = new HashMap<String, String>();
        for (String col : pks) {
            if (!data.containsKey(col)) {
                return false;
            } else {
                pkData.put(col, data.get(col));
            }
        }
        return !checkIfExists(conn, schema, table, pkData);
        // </editor-fold>
    }

    /**
     * NOTE: This does not ensure that there are no PK violations; that must be
     * handled by the caller
     *
     * NOTE 2: The caller must handle transactions if desired; uses the default
     * connection status
     *
     * @param conn Connection to the database to use
     * @param schema Schema to insert into
     * @param table table being inserted into
     * @param data the data in column -> data format
     * @return Returns the id that was inserted into the database; -1 means
     * error 0 means no id returned but success
     */
    public int insertData(Connection conn, String schema, String table, HashMap<String, String> data) {
        // <editor-fold defaultstate="collapsed" desc="insertData">

        // not sure if this should be done here... but lets check to see if the schema table combination exists
        if (!tableExists(conn, schema, table)) {
            System.err.println(String.format("Schema->Table (%s.%s) combination is not valid for insertion", schema, table));
            return -1;
        }

        int id = -1; // error case
        HashMap<String, String> metadata = getTableMetadata(conn, schema, table);
        String returningId = getSerialColumnName(metadata);
        PreparedStatement ps = null;
        String sql = String.format("INSERT INTO %s.%s ", schema, table);
        String columns = "(";
        String values = "VALUES (";
        String returning = returningId == null || returningId.isEmpty() ? "" : String.format("RETURNING %s; ", returningId);
        Set<String> keys = data.keySet();
        ArrayList<String> params = new ArrayList<String>();
        ArrayList<String> paramType = new ArrayList<String>();
        // make the rest of the sql query
        int i = 1;
        int size = keys.size();
        for (String key : keys) {
            columns += i == size ? key + ") " : key + ", ";
            values += i == size ? "?)" : "?, ";
            params.add(data.get(key).replaceAll("\\s+", " ").trim()); // remove returns, tabs, etc and replace as spaces and then remove trailing spaces.
            paramType.add(metadata.get(key));
            i++;
        }
        sql = String.format("%s %s %s %s", sql, columns, values, returning);
        try {
            ps = conn.prepareStatement(sql);
            int q = 1;
            for (String p : params) {
                String pt = paramType.get(q - 1); // ugh... this one uses 0 based...
                if (pt.equalsIgnoreCase("varchar") || pt.equalsIgnoreCase("text") || pt.equalsIgnoreCase("bpchar")) {
                    ps.setString(q, p);
                } else if (pt.equalsIgnoreCase("bool")) {
                    ps.setBoolean(q, Boolean.parseBoolean(p));
                } else if (pt.equalsIgnoreCase("int8") || pt.equalsIgnoreCase("int4") || pt.equalsIgnoreCase("serial") || pt.equalsIgnoreCase("bigserial")) {
                    ps.setInt(q, Integer.parseInt(p));
                } else if (pt.equalsIgnoreCase("timestamp")) {
                    ps.setTimestamp(q, Timestamp.valueOf(p));
                } else if (pt.equalsIgnoreCase("date")) {
                    ps.setDate(q, Date.valueOf(p));
                } else if (pt.equalsIgnoreCase("float4")) {
                    ps.setFloat(q, Float.parseFloat(p));
                } else if (pt.equalsIgnoreCase("float8")) {
                    ps.setDouble(q, Double.parseDouble(p));
                }
                q++;
            }
            ps.executeUpdate();
        } catch (SQLException ex) { // null pointer... error out
            id = -1;
            Logger.getLogger(DatabaseUtils.class.getName()).log(Level.SEVERE, null, ex);
            Logger.getLogger(DatabaseUtils.class.getName()).log(Level.SEVERE, String.format("Error in the following SQL query (%s)", sql));
            Logger.getLogger(DatabaseUtils.class.getName()).log(Level.SEVERE, String.format("The data passed: %s", data.toString()));
        } catch (NullPointerException ex) { // null pointer... error out
            id = -1;
            Logger.getLogger(DatabaseUtils.class.getName()).log(Level.SEVERE, null, ex);
            Logger.getLogger(DatabaseUtils.class.getName()).log(Level.SEVERE, String.format("Error in the following SQL query (%s)", sql));
            Logger.getLogger(DatabaseUtils.class.getName()).log(Level.SEVERE, String.format("The data passed: %s", data.toString()));
        } finally {
            try {
                if (ps != null) {
                    ps.close();
                }
            } catch (SQLException ex) {
                Logger.getLogger(DatabaseUtils.class.getName()).log(Level.SEVERE, String.format("Error setting up or executing %s", sql));
                Logger.getLogger(DatabaseUtils.class.getName()).log(Level.SEVERE, null, ex);
            }
        }
        return id;
        // </editor-fold>
    }

    /**
     * Updates a record(s) with the data provided
     *
     * @param conn
     * @param schema
     * @param table
     * @param where
     * @param data
     * @return Returns the number of records updated
     */
    public int updateRecord(Connection conn, String schema, String table, HashMap<String, String> where, HashMap<String, String> data) {
        // <editor-fold defaultstate="collapsed" desc="updateRecord">
        // not sure if this should be done here... but lets check to see if the schema table combination exists
        if (!tableExists(conn, schema, table)) {
            System.err.println(String.format("Schema->Table (%s.%s) combination is not valid for updating", schema, table));
            return -1;
        }

        int numRecords = -1;
        HashMap<String, String> metadata = getTableMetadata(conn, schema, table);
        PreparedStatement ps = null;
        String up = String.format("UPDATE %s.%s", schema, table);
        String set = " SET ";
        String wh = "";
        Set<String> keys = data.keySet();
        ArrayList<String> params = new ArrayList<String>();
        ArrayList<String> paramType = new ArrayList<String>();
        // make the rest of the sql query
        int i = 1;
        int size = keys.size();
        for (String key : keys) {
            set += String.format("%s = ? ", key) + (i == size ? "" : ", ");
            params.add(data.get(key).replaceAll("\\s+", " ").trim()); // remove returns, tabs, etc and replace as spaces and then remove trailing spaces.
            paramType.add(metadata.get(key));
            i++;
        }
        if (where != null && !where.isEmpty()) {
            wh = "WHERE ";
            Set<String> whereKeys = where.keySet();
            int q = 1;
            for (String key : whereKeys) {
                String pt = metadata.get(key);
                params.add(where.get(key));
                paramType.add(pt);
                if (pt.equalsIgnoreCase("varchar") || pt.equalsIgnoreCase("text") || pt.equalsIgnoreCase("bpchar")) {
                    wh += q != 1 ? String.format("AND LOWER(%s) = LOWER(?) ", key) : String.format("LOWER(%s) = LOWER(?) ", key);
                } else {
                    wh += q != 1 ? String.format("AND %s = ? ", key) : String.format("%s = ? ", key);
                }
                q++;
            }
        }
        String sql = "";
        try {
            sql = String.format("%s %s %s", up, set, wh);
            ps = conn.prepareStatement(sql);
            int q = 1;
            for (String p : params) {
                String pt = paramType.get(q - 1); // ugh... this one uses 0 based...
                if (pt.equalsIgnoreCase("varchar") || pt.equalsIgnoreCase("text") || pt.equalsIgnoreCase("bpchar")) {
                    ps.setString(q, p);
                } else if (pt.equalsIgnoreCase("bool")) {
                    if (p.isEmpty()) {
                        ps.setNull(q, java.sql.Types.BOOLEAN);
                    } else {
                        ps.setBoolean(q, Boolean.parseBoolean(p));
                    }
                } else if (pt.equalsIgnoreCase("int8") || pt.equalsIgnoreCase("int4") || pt.equalsIgnoreCase("serial") || pt.equalsIgnoreCase("bigserial")) {
                    if (p.isEmpty()) {
                        ps.setNull(q, java.sql.Types.INTEGER);
                    } else {
                        ps.setInt(q, Integer.parseInt(p));
                    }
                } else if (pt.equalsIgnoreCase("timestamp")) {
                    if (p.isEmpty()) {
                        ps.setNull(q, java.sql.Types.TIMESTAMP);
                    } else {
                        ps.setTimestamp(q, Timestamp.valueOf(p));
                    }
                } else if (pt.equalsIgnoreCase("date")) {
                    if (p.isEmpty()) {
                        ps.setNull(q, java.sql.Types.DATE);
                    } else {
                        ps.setDate(q, Date.valueOf(p));
                    }
                } else if (pt.equalsIgnoreCase("float4")) {
                    if (p.isEmpty()) {
                        ps.setNull(q, java.sql.Types.FLOAT);
                    } else {
                        ps.setFloat(q, Float.parseFloat(p));
                    }
                } else if (pt.equalsIgnoreCase("float8")) {
                    if (p.isEmpty()) {
                        ps.setNull(q, java.sql.Types.DOUBLE);
                    } else {
                        ps.setDouble(q, Double.parseDouble(p));
                    }
                }
                q++;
            }
            numRecords = ps.executeUpdate();
            System.out.println(String.format("%s record(s) updated (%s->%s)", numRecords, schema, table));
        } catch (SQLException ex) { // null pointer... error out
            numRecords = -1;
            Logger.getLogger(DatabaseUtils.class.getName()).log(Level.SEVERE, null, ex);
            Logger.getLogger(DatabaseUtils.class.getName()).log(Level.SEVERE, String.format("Error in the following SQL query (%s)", sql));
            Logger.getLogger(DatabaseUtils.class.getName()).log(Level.SEVERE, String.format("The data passed: %s", data.toString()));
        } catch (NullPointerException ex) { // null pointer... error out
            numRecords = -1;
            Logger.getLogger(DatabaseUtils.class.getName()).log(Level.SEVERE, null, ex);
            Logger.getLogger(DatabaseUtils.class.getName()).log(Level.SEVERE, String.format("Error in the following SQL query (%s)", sql));
            Logger.getLogger(DatabaseUtils.class.getName()).log(Level.SEVERE, String.format("The data passed: %s", data.toString()));
        } finally {
            try {
                if (ps != null) {
                    ps.close();
                }
            } catch (SQLException ex) {
                Logger.getLogger(DatabaseUtils.class.getName()).log(Level.SEVERE, null, ex);
            }
        }
        return numRecords;
        // </editor-fold>
    }

    /**
     * Remove records based on the items in the where clause hashmap.
     *
     * @param conn
     * @param schema
     * @param table
     * @param where
     * @return Returns the number of records deleted
     */
    public int deleteRecord(Connection conn, String schema, String table, HashMap<String, String> where) {
        // <editor-fold defaultstate="collapsed" desc="deleteRecord">
        int numRecords = -1;
        // not sure if this should be done here... but lets check to see if the schema table combination exists
        if (!tableExists(conn, schema, table)) {
            System.err.println(String.format("Schema->Table (%s.%s) combination is not valid for deletion", schema, table));
            return -1;
        }

        ResultSet rs = null;
        PreparedStatement ps = null;
        ArrayList<String> params = new ArrayList<String>();
        ArrayList<String> paramType = new ArrayList<String>();
        String delete = String.format("DELETE FROM %s.%s", schema, table);
        String wh = "";
        String returning = "Returning *"; // this is postgres specific...
        if (where != null && !where.isEmpty()) {
            HashMap<String, String> metadata = getTableMetadata(conn, schema, table);
            wh += "WHERE ";
            Set<String> whereKeys = where.keySet();
            int q = 1;
            for (String key : whereKeys) {
                String pt = metadata.get(key);
                params.add(where.get(key));
                paramType.add(pt);
                if (pt.equalsIgnoreCase("varchar") || pt.equalsIgnoreCase("text") || pt.equalsIgnoreCase("bpchar")) {
                    wh += q != 1 ? String.format("AND LOWER(%s) = LOWER(?) ", key) : String.format("LOWER(%s) = LOWER(?) ", key);
                } else {
                    wh += q != 1 ? String.format("AND %s = ? ", key) : String.format("%s = ? ", key);
                }
                q++;
            }
        } else {
            where = new HashMap<String, String>();
        }
        String sql = "";
        try {
            sql = String.format("%s %s %s", delete, wh, returning);
            ps = conn.prepareStatement(sql);
            int q = 1;
            for (String p : params) {
                String pt = paramType.get(q - 1); // ugh... this one uses 0 based...
                if (pt.equalsIgnoreCase("varchar") || pt.equalsIgnoreCase("text") || pt.equalsIgnoreCase("bpchar")) {
                    ps.setString(q, p);
                } else if (pt.equalsIgnoreCase("bool")) {
                    ps.setBoolean(q, Boolean.parseBoolean(p));
                } else if (pt.equalsIgnoreCase("int8") || pt.equalsIgnoreCase("int4") || pt.equalsIgnoreCase("serial") || pt.equalsIgnoreCase("bigserial")) {
                    ps.setInt(q, Integer.parseInt(p));
                } else if (pt.equalsIgnoreCase("timestamp")) {
                    ps.setTimestamp(q, Timestamp.valueOf(p));
                } else if (pt.equalsIgnoreCase("date")) {
                    ps.setDate(q, Date.valueOf(p));
                }
                q++;
            }
            rs = ps.executeQuery();
            ArrayList<HashMap<String, String>> rec = convertResultSetToArrayList(rs);
            numRecords = rec.isEmpty() ? -1 : rec.size();
            System.out.println(String.format("%s record(s) deleted (%s->%s)", numRecords, schema, table));
        } catch (SQLException ex) {
            numRecords = -1;
            Logger.getLogger(DatabaseUtils.class.getName()).log(Level.SEVERE, null, ex);
            Logger.getLogger(DatabaseUtils.class.getName()).log(Level.SEVERE, String.format("Error in the following SQL query (%s)", sql));
            Logger.getLogger(DatabaseUtils.class.getName()).log(Level.SEVERE, String.format("The where clause passed: %s", where.toString()));
        } finally {
            try {
                if (ps != null) {
                    ps.close();
                }
                if (rs != null) {
                    rs.close();
                }
            } catch (SQLException ex) {
                Logger.getLogger(DatabaseUtils.class.getName()).log(Level.SEVERE, null, ex);
            }
        }
        return numRecords;
        // </editor-fold>
    }

    /**
     * Returns an ArrayList<HashMap<String, String>> of values from the columns
     * provided along with counts of each record
     *
     * @param conn
     * @param schema
     * @param table
     * @param columns
     * @return
     */
    public ArrayList<HashMap<String, String>> getDistinctColumnRecords(Connection conn, String schema, String table, ArrayList<String> columns) {
        // <editor-fold defaultstate="collapsed" desc="getDistinctColumnRecords">
        // not sure if this should be done here... but lets check to see if the schema table combination exists
        if (!tableExists(conn, schema, table)) {
            System.err.println(String.format("Schema->Table (%s.%s) combination is not valid for getting distinct column records", schema, table));
            return new ArrayList<HashMap<String, String>>();
        }
        // build the sql statement removing nulls, etc.
        String select = "SELECT DISTINCT ";
        String order = "ORDER BY ";
        String where = "WHERE ";
        String group = "GROUP BY ";
        int i = 0;
        for (String col : columns) {
            select += String.format("COALESCE(%s, '') AS %s ", col, col);
            order += String.format("COALESCE(%s, '') ", col);
            where += String.format("COALESCE(%s, '')  '' ", col);
            group += String.format("COALESCE(%s, '') ", col);
            i++;
            if (i != columns.size()) {
                select += ", ";
                order += ", ";
                where += " OR ";
                group += ", ";
            } else {
                select += ", COUNT(*) AS num_records ";
            }
        }
        String sql = String.format("%s FROM %s.%s %s %s %s;", select, schema, table, where, group, order);
        ArrayList<HashMap<String, String>> rows = executeSQL(conn, sql);
        return rows;
        // </editor-fold>
    }

    /**
     * Returns an ArrayList<String> of values from the column provided (good for
     * autocomplete, etc)
     *
     * @param conn
     * @param schema
     * @param table
     * @param column
     * @return
     */
    public ArrayList<String> getDistinctColumnRecords(Connection conn, String schema, String table, String column) {
        // <editor-fold defaultstate="collapsed" desc="getDistinctColumnRecords">
        // not sure if this should be done here... but lets check to see if the schema table combination exists
        if (!tableExists(conn, schema, table)) {
            System.err.println(String.format("Schema->Table (%s.%s) combination is not valid for getting distinct column records", schema, table));
            return new ArrayList<String>();
        }

        ArrayList<String> strs = new ArrayList<String>();
        String sql = String.format("SELECT DISTINCT COALESCE(%s, '') AS val FROM %s.%s WHERE COALESCE(%s, '')  '' ORDER BY COALESCE(%s, '') ASC;", column, schema, table, column, column);
        ArrayList<HashMap<String, String>> rows = executeSQL(conn, sql);
        for (HashMap<String, String> row : rows) {
            strs.add(row.get("val"));
        }
        return strs;
        // </editor-fold>
    }

    /**
     * Returns a correctly formatted object to be used to build a sort.
     *
     * @param columnName
     * @param direction Accepted values: asc and desc, otherwise defaults to asc
     * @return
     */
    public HashMap<String, String> getSortItem(String columnName, String direction) {
        // <editor-fold defaultstate="collapsed" desc="getSortItem">
        HashMap<String, String> h = new HashMap<String, String>();
        h.put("column", columnName);
        h.put("direction", direction.equalsIgnoreCase("desc") ? "DESC" : "ASC");
        return h;
        // </editor-fold>
    }

    /**
     * Returns the same value as now()
     *
     * @return
     */
    public String getTimeStamp() {
        return String.valueOf(new Timestamp(System.currentTimeMillis()));
    }

    /**
     * Returns the same value as now()
     *
     * @return
     */
    public String getDate() {
        return String.valueOf(new java.sql.Date(System.currentTimeMillis()));
    }

    /**
     * Takes a string and parameters and executes; this is used to help control
     * and clean up prepared statements and resultsets; only allows select
     * statements
     *
     * Note: does not close the connection
     *
     * Note 2: SQL String must be "SELECT ...." to be valid.
     *
     * @param conn
     * @param sql
     * @param params
     * @param paramType
     * @return
     */
    public ArrayList<HashMap<String, String>> executeSQL(Connection conn, String sql, ArrayList<String> params, ArrayList<String> paramType) {
        // <editor-fold defaultstate="collapsed" desc="executeSQL">
        // ensure it begins with "SELECT" return nothing if it is not a select statement
        if (!sql.trim().substring(0, 6).equalsIgnoreCase("select")) {
            System.err.println(String.format("SQL Statement (%s) is not a 'SELECT' statement. Not continuing.", sql));
            return new ArrayList<HashMap<String, String>>();
        }

        if (params == null) {
            params = new ArrayList<String>();
        }
        if (paramType == null) {
            paramType = new ArrayList<String>();
        }
        return sqlExecution(conn, sql, params, paramType);
        // </editor-fold>
    }

    private ArrayList<HashMap<String, String>> sqlExecution(Connection conn, String sql, ArrayList<String> params, ArrayList<String> paramType) {
        // <editor-fold defaultstate="collapsed" desc="sqlExecution">
        ArrayList<HashMap<String, String>> results = new ArrayList<HashMap<String, String>>();
        ResultSet rs = null;
        PreparedStatement ps = null;
        try {
            ps = conn.prepareStatement(sql);
            int q = 1;
            for (String p : params) {
                String pt = paramType.get(q - 1); // ugh... this one uses 0 based...
                if (pt.equalsIgnoreCase("varchar") || pt.equalsIgnoreCase("text") || pt.equalsIgnoreCase("bpchar")) {
                    ps.setString(q, p);
                } else if (pt.equalsIgnoreCase("bool")) {
                    ps.setBoolean(q, Boolean.parseBoolean(p));
                } else if (pt.equalsIgnoreCase("int8") || pt.equalsIgnoreCase("int4") || pt.equalsIgnoreCase("serial") || pt.equalsIgnoreCase("bigserial")) {
                    ps.setInt(q, Integer.parseInt(p));
                } else if (pt.equalsIgnoreCase("timestamp")) {
                    ps.setTimestamp(q, Timestamp.valueOf(p));
                } else if (pt.equalsIgnoreCase("date")) {
                    ps.setDate(q, Date.valueOf(p));
                } else if (pt.equalsIgnoreCase("float4")) {
                    ps.setFloat(q, Float.parseFloat(p));
                } else if (pt.equalsIgnoreCase("float8")) {
                    ps.setDouble(q, Double.parseDouble(p));
                }
                q++;
            }
            rs = ps.executeQuery();
            results = convertResultSetToArrayList(rs);
        } catch (SQLException ex) {
            Logger.getLogger(DatabaseUtils.class.getName()).log(Level.SEVERE, null, ex);
            Logger.getLogger(DatabaseUtils.class.getName()).log(Level.SEVERE, String.format("Error in the following SQL query (%s)", sql));
            Logger.getLogger(DatabaseUtils.class.getName()).log(Level.SEVERE, String.format("The params passed: %s", params.toString()));
        } finally {
            try {
                if (rs != null) {
                    rs.close();
                }
                if (ps != null) {
                    ps.close();
                }
            } catch (SQLException ex) {
                Logger.getLogger(DatabaseUtils.class.getName()).log(Level.SEVERE, null, ex);
            }
        }
        return results;
        // </editor-fold>
    }

    /**
     * Execute SQL where there are no parameters to handle
     *
     * @param conn
     * @param sql
     * @return
     */
    public ArrayList<HashMap<String, String>> executeSQL(Connection conn, String sql) {
        return executeSQL(conn, sql, null, null);
    }

    /**
     * Makes a portion of the where clause that can be used for querying in full
     * text search mode. Adds a parameter at the end that must be handled by the
     * caller
     *
     * Note: Does NOT add "WHERE" or "AND"
     *
     * @param rows
     * @return
     */
    public String generateFullSearchWhereClause(ArrayList<String> rows) {
        // <editor-fold defaultstate="collapsed" desc="generateFullSearchWhereClause">
        String str = "to_tsvector('english_no_stopwords', REPLACE(";
        for (String s : rows) {
            str += String.format("COALESCE(%s, '')  || ' ' || ", s);
        }
        str += " '', '/', ' ')) @@ plainto_tsquery('english_no_stopwords', ?)";
        return str;
        // </editor-fold>
    }

    /**
     * TODO: Make sure this work... in progress
     *
     * @param conn
     * @param schema
     * @param table
     * @return
     */
    public ArrayList<HashMap<String, String>> getForeignKeys(Connection conn, String schema, String table) {
        // <editor-fold defaultstate="collapsed" desc="getForeignKeys">
        String sql = "SELECT\n"
                + "    tc.table_schema,                                -- the schema of the table that uses the reference\n"
                + "    tc.table_name,                                  -- the table of the table that uses the reference\n"
                + "    tc.constraint_name,                             -- the name of the constraint (nice to have info)    \n"
                + "    kcu.column_name,                                -- the column that is fkey'd \n"
                + "    ccu.table_schema AS foreign_table_schema,       -- the schema of the table that is the source of the fkey\n"
                + "    ccu.table_name AS foreign_table_name,           -- the table name of the table that is the source of the fkey\n"
                + "    kcu.column_name AS foreign_column_name          -- the column of the table that is the source of the fkey\n"
                + "FROM information_schema.table_constraints AS tc \n"
                + "JOIN information_schema.key_column_usage AS kcu ON tc.constraint_name = kcu.constraint_name\n"
                + "JOIN information_schema.constraint_column_usage AS ccu ON ccu.constraint_name = tc.constraint_name\n"
                + "WHERE constraint_type = ? \n"
                + "    AND ccu.table_schema = ?'\n"
                + "    AND ccu.table_name = ?;";
        ArrayList<String> params = new ArrayList<String>();
        ArrayList<String> paramType = new ArrayList<String>();
        params.add("FOREIGN KEY");
        params.add(schema);
        params.add(table);

        return executeSQL(conn, sql, params, paramType);
        // </editor-fold>
    }

    public int deleteCascade(Connection conn, String schema, String table, HashMap<String, String> where) {
        // get the records that are to be deleted

        // find out if there are any fkey issues
        // loop over each fkey and find out if there are things to delete there....
        // for each fkey that needs to be removed, make sure there isn't ANOTHER One that needs to be deleted first.
        // keep doing this??? recursively???
        // delete this record at the bottom, and roll back up
        // delete this record
        // now delete the top level record
        // return the number of records from the main table deleted...
        return -1;
    }
}
