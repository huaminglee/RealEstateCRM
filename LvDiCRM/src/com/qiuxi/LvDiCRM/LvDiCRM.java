/*
       Licensed to the Apache Software Foundation (ASF) under one
       or more contributor license agreements.  See the NOTICE file
       distributed with this work for additional information
       regarding copyright ownership.  The ASF licenses this file
       to you under the Apache License, Version 2.0 (the
       "License"); you may not use this file except in compliance
       with the License.  You may obtain a copy of the License at

         http://www.apache.org/licenses/LICENSE-2.0

       Unless required by applicable law or agreed to in writing,
       software distributed under the License is distributed on an
       "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY
       KIND, either express or implied.  See the License for the
       specific language governing permissions and limitations
       under the License.
 */

package com.qiuxi.LvDiCRM;   

import org.apache.cordova.CordovaActivity;

import android.os.Bundle;
import android.view.Menu;
import android.view.MenuItem;

public class LvDiCRM extends CordovaActivity {
	@Override
	public void onCreate(Bundle savedInstanceState) {
		super.onCreate(savedInstanceState);
		super.init();
		// Set by <content src="index.html" /> in config.xml
		// super.loadUrl(Config.getStartUrl());
		// super.loadUrl("http://192.168.1.110:19717/");
		// super.loadUrl("http://180.166.191.160/crmtest");
		// super.loadUrl("http://www.3drp.cn/sort/0-0-0-0-1");
		super.loadUrl("file:///android_asset/www/index.html");
	}   

	@Override
	public boolean onCreateOptionsMenu(Menu menu) {
		// menu.add(0,1,1,"是是是");
		// menu.add(0,2,2,"啊啊" );
		// menu.add(0,3,3,"退出");
		menu.add(0, 1, 1, "退出");
		return super.onCreateOptionsMenu(menu);
	}

	@Override
	public boolean onOptionsItemSelected(MenuItem item) {
		// if(item.getItemId()==3){
		// finish();
		// }
		// if(item.getItemId()==1){
		// super.loadUrl("file:///android_asset/www/spec.html");
		// }
		// if(item.getItemId()==2){
		// super.loadUrl("file:///android_asset/www/index.html");
		// }
		if (item.getItemId() == 1) {
			finish();
		}
		return super.onOptionsItemSelected(item);
	}
}
