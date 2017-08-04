using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;
using INCZONE.Repositories;
using INCZONE.Managers;
using log4net;
using INCZONE.Common;
using INCZONE.Model;
using System.Net.Http.Headers;
using Newtonsoft.Json;

namespace INCZONE.Forms.Configuration
{
    public partial class MapDataForm : Form
    {
        readonly IUnitOfWork _uow;
        private static readonly ILog log = LogManager.GetLogger(SystemConstants.Logger_Ref);

        public MapDataForm(IncZoneMDIParent incZoneMDIParent)
        {
            InitializeComponent();

            refreshMapSetsBt.Enabled = false;
            this.MdiParent = incZoneMDIParent;
//            log.Debug("In CapWINForm Constructor");
            string connectionString = ConfigurationManager.ConnectionStrings["IncZoneEntities"].ConnectionString;
            this._uow = new UnitOfWork(connectionString);
            
        }

        private async Task<string> getMapSets()
        {
//            log.Debug("In getMapSets");

            string getStringTask = string.Empty;

            try
            {                
                using (var client = new HttpClient())
                {
                    client.BaseAddress = new Uri("http://inczonemap.cloudapp.net/");
                    client.DefaultRequestHeaders.Accept.Clear();
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/xml"));

                    HttpResponseMessage response = await client.GetAsync("api/MapSet/GetmapSets");
                    if (response.IsSuccessStatusCode)
                    {
                        getStringTask = await response.Content.ReadAsStringAsync();
                    }
                }
            }
            catch (NotSupportedException e)
            {
                log.Error("NotSupportedException", e);
                throw e;
            }
            catch (ArgumentNullException e)
            {
                log.Error("ArgumentNullException", e);
                throw e;
            }
            catch (WebException e)
            {
                log.Error("WebException", e);
                throw e;
            }
            catch (XmlException e)
            {
                log.Error("XmlException", e);
                throw e;
            }
            catch (Exception e)
            {
                log.Error("Exception", e);
                throw e;
            }

            return getStringTask;
        }

        private async Task<List<INCZONE.Common.MapNode>> getMapNodes(Guid Id)
        {
//            log.Debug("In getMapNodes");

            List<INCZONE.Common.MapNode> MapNodesList = new List<INCZONE.Common.MapNode>();

            try
            {
                if (Id != Guid.Empty)
                {
                    using (var client = new HttpClient())
                    {
                        client.MaxResponseContentBufferSize = 2147483647;
                        client.BaseAddress = new Uri("http://inczonemap.cloudapp.net/");
                        client.DefaultRequestHeaders.Accept.Clear();
                        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                        HttpResponseMessage response = await client.GetAsync("api/MapNode/GetmapNodes/" + Id);

                        if (response.IsSuccessStatusCode)
                        {
                            MapNodesList = JsonConvert.DeserializeObject<List<INCZONE.Common.MapNode>>(response.Content.ReadAsStringAsync().Result);
                        }
                    }
                }
            }
            catch (NotSupportedException e)
            {
                log.Error("NotSupportedException", e);
                throw e;
            }
            catch (ArgumentNullException e)
            {
                log.Error("ArgumentNullException", e);
                throw e;
            }
            catch (WebException e)
            {
                log.Error("WebException", e);
                throw e;
            }
            catch (XmlException e)
            {
                log.Error("XmlException", e);
                throw e;
            }
            catch (Exception e)
            {
                log.Error("Exception", e);
                throw e;
            }

            return MapNodesList;
        }

        private async Task<List<INCZONE.Common.MapLink>> getMapLinks(Guid Id)
        {
//            log.Debug("In getMapNodes");

            List<INCZONE.Common.MapLink> MapLinkList = new List<INCZONE.Common.MapLink>();

            try
            {
                if (Id != Guid.Empty)
                {
                    using (var client = new HttpClient())
                    {
                        client.MaxResponseContentBufferSize = 2147483647;
                        client.BaseAddress = new Uri("http://inczonemap.cloudapp.net/");
                        client.DefaultRequestHeaders.Accept.Clear();
                        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                        HttpResponseMessage response = await client.GetAsync("api/MapLink/GetmapLinks/" + Id);

                        if (response.IsSuccessStatusCode)
                        {
                            MapLinkList = JsonConvert.DeserializeObject<List<INCZONE.Common.MapLink>>(response.Content.ReadAsStringAsync().Result);
                        }
                    }
                }
            }
            catch (NotSupportedException e)
            {
                log.Error("NotSupportedException", e);
                throw e;
            }
            catch (ArgumentNullException e)
            {
                log.Error("ArgumentNullException", e);
                throw e;
            }
            catch (WebException e)
            {
                log.Error("WebException", e);
                throw e;
            }
            catch (XmlException e)
            {
                log.Error("XmlException", e);
                throw e;
            }
            catch (Exception e)
            {
                log.Error("Exception", e);
                throw e;
            }

            return MapLinkList;
        }

        private async void mapSetCB_SelectedIndexChanged(object sender, EventArgs e)
        {
            loading load = new loading();

            try
            {
                if (mapSetCB.SelectedIndex > 0)
                {
                    Guid mapSetId = (Guid)mapSetCB.SelectedValue;
                    load.Show();

                    IncZoneMDIParent._SelectedMapSet = mapSetId;
                    IncZoneMDIParent._LoadedMapName = mapSetCB.Text;

                    List<INCZONE.Common.MapNode> MapNodesList = await getMapNodes(mapSetId);
                    List<INCZONE.Common.MapLink> MapLinkList = await getMapLinks(mapSetId);

                    if (MapNodesList != null && MapNodesList.Count() > 0)
                    {
                        refreshMapSetsBt.Enabled = false;
                        //await saveMapNodes(MapNodesList, mapSetId);
                        textBox1.Text = "Map Nodes successfuly loaded";
                        IncZoneMDIParent.MapNodeList = MapNodesList;

                        if (MapLinkList != null && MapLinkList.Count() > 0)
                        {
                            refreshMapSetsBt.Enabled = false;
                            textBox1.Text = "Map Links successfuly loaded";
                            IncZoneMDIParent.MapLinkList = MapLinkList;
                        }
                        else
                        {
                            refreshMapSetsBt.Enabled = true;
                            throw new Exception("Map Nodes are null");
                        }
                    }
                    else
                    {
                        refreshMapSetsBt.Enabled = true;
                        throw new Exception("Map Nodes are null");
                    }
                }
                else
                {
                    refreshMapSetsBt.Enabled = true;
                }
            }
            catch (Exception ex)
            {
                log.Error("mapSetCB_SelectedIndexChanged Exception", ex);
                textBox1.Text = "Failed to load the map node";
                MessageBox.Show("The Map Node could not be loaded, an error occured", SystemConstants.MessageBox_Caption_Error, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                load.Close();
            }
        }

        private void _updateText(string action, int total, int counter)
        {
            _updateText(string.Format(action + " # {0} of {1}", counter, total));
        }

        private async void MapDataForm_Load(object sender, EventArgs e)
        {
            refreshMapSetsBt.Enabled = false;
            mapSetCB.Items.Clear();
            mapSetCB.DataSource = null;
            loading load = new loading();
            textBox1.Text = "Map Sets loading";

            try
            {
                load.Show();
                string xml = await getMapSets();
                XmlDocument MapSetXmlDocument = MapRepositoryManager.GetXmlDocument(xml);
                ArrayOfmapSet _MapSets = MapRepositoryManager.MapSetDigester(MapSetXmlDocument);

                if (_MapSets != null)
                {
                    refreshMapSetsBt.Enabled = false;

                    mapSetCB.DisplayMember = "name";
                    mapSetCB.ValueMember = "Id";
                    List<INCZONE.Common.MapSet> list = _MapSets.MapSetList;

                    foreach (INCZONE.Common.MapSet var in _MapSets.MapSetList)
                    {
                        _uow.MapSets.Add(new INCZONE.Model.MapSet()
                        {
                            GuId = var.Id,
                            description = var.description,
                            name = var.name
                        }
                        );
                        _uow.Commit();
                    }
                        

                    list.Add(new INCZONE.Common.MapSet(Guid.Empty, "Please Select a Map Set....", ""));

                    mapSetCB.DataSource = list.OrderBy(m => m.Id).ToList();

                    textBox1.Text = "Map Sets successfuly loaded";
                    refreshMapSetsBt.Enabled = true;
                }
                else
                {
                    refreshMapSetsBt.Enabled = true;
                    throw new Exception("No Map Sets were returned");
                }                
            }
            catch (Exception ex)
            {
                log.Error("MapDataForm_Load Exception", ex);
                textBox1.Text = "Failed to load the map sets";
                refreshMapSetsBt.Enabled = true;
                MessageBox.Show("The Map Sets could not be initialized, an error occured", SystemConstants.MessageBox_Caption_Error, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                load.Close();
            }
        }

        private async void refreshMapSetsBt_Click(object sender, EventArgs e)
        {
            try
            {
                refreshMapSetsBt.Enabled = false;
                mapSetCB.DataSource = null;
                if (mapSetCB.Items != null && mapSetCB.Items.Count > 0)
                    mapSetCB.Items.Clear();
                
                textBox1.Text = "Map Sets reloading";

                string xml = await getMapSets();
                XmlDocument MapSetXmlDocument = MapRepositoryManager.GetXmlDocument(xml);
                ArrayOfmapSet _MapSets = MapRepositoryManager.MapSetDigester(MapSetXmlDocument);

                if (_MapSets != null)
                {
                    refreshMapSetsBt.Enabled = false;

                    mapSetCB.DisplayMember = "name";
                    mapSetCB.ValueMember = "Id";
                    List<INCZONE.Common.MapSet> list = _MapSets.MapSetList;

                    foreach (INCZONE.Common.MapSet var in _MapSets.MapSetList)
                    {
                        try
                        {
                            INCZONE.Model.MapSet entity =_uow.MapSets.FindWhere(m => m.GuId == var.Id).FirstOrDefault();
                            if (entity == null)
                            {
                                _uow.MapSets.Add(new INCZONE.Model.MapSet()
                                {
                                    GuId = var.Id,
                                    description = var.description,
                                    name = var.name
                                }
                                );
                                _uow.Commit();
                            }
                        }
                        catch (Exception ex)
                        {
                            log.Info("Dublicate Excepotion");
                        }
                    }


                    list.Add(new INCZONE.Common.MapSet(Guid.Empty, "Please Select a Map Set....", ""));

                    mapSetCB.DataSource = list.OrderBy(m => m.Id).ToList();

                    textBox1.Text = "Map Sets successfuly reloaded";
                    refreshMapSetsBt.Enabled = true;
                }
                else
                {
                    refreshMapSetsBt.Enabled = true;
                    throw new Exception("No Map Sets were returned");
                }
            }
            catch (Exception ex)
            {
                log.Error("refreshMapSetsBt_Click Exception", ex);
                textBox1.Text = "Failed to reload the map sets";
                refreshMapSetsBt.Enabled = true;
                MessageBox.Show("The Map Sets could not be initialized, an error occured", SystemConstants.MessageBox_Caption_Error, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                this.Enabled = true;
                this.MdiParent.Enabled = true;
            }
        }

        private async Task saveMapNodes(List<INCZONE.Common.MapNode> MapNodesList, Guid mapSetId ) 
        {
//            log.Debug("In saveMapNodes");
            int counter = 1;

            try
            {
                await Task.Run(() =>
                {
                    _updateText("Determine if existing map nodes need deleting loaded Map Nodes");

                    List<INCZONE.Model.MapNode> temp = _uow.MapNodes.FindWhere(m => m.mapSetGuId == mapSetId).ToList();

                    if (temp != null && temp.Count > 0)
                    {
                        foreach (INCZONE.Model.MapNode mapNode in temp)
                        {
                            _updateText("Deleting loaded Map Nodes", MapNodesList.Count, counter);
                            _uow.MapNodes.Remove(mapNode);
                            _uow.Commit();
                            counter++;
                        }
                    }

                    counter = 1;
                    Model.MapSet mapSetEntity = _uow.MapSets.FindWhere(m => m.GuId == mapSetId).FirstOrDefault();
                    foreach (INCZONE.Common.MapNode mapNode in MapNodesList)
                    {
                        _updateText("Saving selected Map Nodes",MapNodesList.Count, counter);

                        INCZONE.Model.MapNode MapNode = new INCZONE.Model.MapNode()
                        {
                            directionality = mapNode.directionality,
                            distance = mapNode.distance,
                            elevation = mapNode.elevation,
                            GuId = mapNode.Id,
                            laneOrder = mapNode.laneOrder,
                            laneWidth = mapNode.laneWidth,
                            lat = mapNode.latitude,
                            @long = mapNode.longitude,
                            mapSetGuId = mapNode.mapSetId,
                            mapSetId = mapSetEntity.Id,
                            positionalAccuracyP1 = mapNode.positionalAccuracyP1,
                            positionalAccuracyP2 = mapNode.positionalAccuracyP2,
                            positionalAccuracyP3 = mapNode.positionalAccuracyP3,
                            postedSpeed = mapNode.postedSpeed,
                            xOffset = mapNode.xOffset,
                            yOffset = mapNode.yOffset,
                            zOffset = mapNode.zOffset
                        };

                        _uow.MapNodes.Add(MapNode);
                        _uow.Commit();
                        counter++;
                    }
                }
            );
            }
            catch (Exception e)
            {
                log.Error("Exception", e);
                throw e;
            }
        }

        private void _updateText(string p)
        {
            Func<int> del = delegate()
            {
                textBox1.Text = p;
                return 0;
            };

            Invoke(del);
        }
    }
}
