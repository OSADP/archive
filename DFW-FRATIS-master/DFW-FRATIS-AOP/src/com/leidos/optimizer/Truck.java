/**
Copyright 2015 Leidos Corp

  Licensed under the Apache License, Version 2.0 (the "License");
  you may not use this file except in compliance with the License.
  You may obtain a copy of the License at

      http://www.apache.org/licenses/LICENSE-2.0

  Unless required by applicable law or agreed to in writing, software
  distributed under the License is distributed on an "AS IS" BASIS,
  WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
  See the License for the specific language governing permissions and
  limitations under the License.
*/

package com.leidos.optimizer;

import java.util.ArrayList;
import java.util.List;

import com.leidos.optimizer.data.Optimizable;

/**
 *
 * @author cassadyja
 */
public class Truck {
    private List<Optimizable> moves;


    public Truck(){
        this.moves = new ArrayList<Optimizable>();
    }


    public void addMove(Optimizable move){
        moves.add(move);
    }

    public Optimizable[] getMoves(){
        Optimizable[] moveArray = new Optimizable[moves.size()];
        moves.toArray(moveArray);
        return moveArray;
    }

    public void setMoves(List<Optimizable> moves){
        this.moves = moves;
    }


}
