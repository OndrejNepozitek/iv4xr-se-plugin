/*
This program has been developed by students from the bachelor Computer Science
at Utrecht University within the Software and Game project course.

©Copyright Utrecht University (Department of Information and Computing Sciences)
*/
package agents.demo;

import agents.LabRecruitsTestAgent;
import agents.TestSettings;
import agents.tactics.GoalLib;
import environments.EnvironmentConfig;
import environments.LabRecruitsEnvironment;
import game.LabRecruitsTestServer;
import helperclasses.datastructures.Vec3;
import helperclasses.datastructures.linq.QArrayList;
import logger.JsonLoggerInstrument;
import logger.PrintColor;
import nl.uu.cs.aplib.mainConcepts.Environment;

import org.junit.jupiter.api.AfterAll;
import org.junit.jupiter.api.BeforeAll;
import org.junit.jupiter.api.Test;

import game.Platform;
import world.BeliefState;

import static agents.TestSettings.*;
import static nl.uu.cs.aplib.AplibEDSL.SEQ;

import java.util.Scanner;

public class FireHazardAgentDirect {

    private static LabRecruitsTestServer labRecruitsTestServer;

    @BeforeAll
    static void start() {
    	// Uncomment this to make the game's graphic visible:
    	TestSettings.USE_GRAPHICS = true ;
    	String labRecruitesExeRootDir = System.getProperty("user.dir") ;
    	labRecruitsTestServer = TestSettings.start_LabRecruitsTestServer(labRecruitesExeRootDir) ;
    }

    @AfterAll
    static void close() {
        if(labRecruitsTestServer!=null) labRecruitsTestServer.close();
    }

    void instrument(Environment env) {
    	env.registerInstrumenter(new JsonLoggerInstrument()).turnOnDebugInstrumentation();
    }
    
    /**
     * This demo will tests if an agent can escape the fire hazard level
     * 
     * BROKEN!! Fix this the agent get stuck.
     */
    //@Test
    public void fireHazardDemo() throws InterruptedException{
        //Add the level to the resources and change the string in the environmentConfig on the next line from Ramps to the new level
        var env = new LabRecruitsEnvironment(new EnvironmentConfig("HZRDDirect").replaceAgentMovementSpeed(0.2f));
        if(USE_INSTRUMENT) instrument(env) ;
        
        QArrayList<LabRecruitsTestAgent> agents = new QArrayList<>(new LabRecruitsTestAgent[] {
                createHazardAgent(env)
        });

    	if(TestSettings.USE_GRAPHICS) {
    		System.out.println("You can drag then game window elsewhere for beter viewing. Then hit RETURN to continue.") ;
    		new Scanner(System.in) . nextLine() ;
    	}
    	
        // press play in Unity
        if (! env.startSimulation())
            throw new InterruptedException("Unity refuses to start the Simulation!");

        int tick = 0;
        // run the agent until it solves its goal:
        while (!agents.allTrue(LabRecruitsTestAgent::success)){

            System.out.println(PrintColor.GREEN("TICK " + tick + ":"));

            // only updates in progress
            for(var agent : agents.where(agent -> !agent.success())){
                agent.update();
                if(agent.success()){
                    agent.printStatus();
                }
            }
            Thread.sleep(30);
            tick++;
        }

        if (!env.close())
            throw new InterruptedException("Unity refuses to close the Simulation!");
    }

    /**
     * This method will create an agent which will move through the fire hazard level
     */
    public static LabRecruitsTestAgent createHazardAgent(LabRecruitsEnvironment env){
        LabRecruitsTestAgent agent = new LabRecruitsTestAgent("0", "")
                                     . attachState(new BeliefState())
                                     . attachEnvironment(env);

        //the goals are in order
        //add move goals with GoalStructureFactory.reachPositions(new Vec3(1,0,1)) it can take either a single or multiple vec3
        //set interaction with the buttons with GoalStructureFactory.reachAndInteract("CB3") using the id of the button
        //You will probably not want to explore to prevent random behaviour. in order to do this set the waypoints close enough to each other

        agent.setGoal(SEQ(
                GoalLib.positionsVisited(new Vec3(6,0,5),
                                                    new Vec3(7,0,8),
                                                    new Vec3(7,0,11),
                                                    new Vec3(5,0,11),
                                                    new Vec3(5,0,16),
                                                    new Vec3(2,0,16),
                                                    new Vec3(1,0,18),
                                                    new Vec3(3,0,20),
                                                    new Vec3(6,0,20)),
                GoalLib.entityReachedAndInteracted("b1.1"),
                GoalLib.positionsVisited(new Vec3(5,0,25))
                ));
        return agent;
    }
}
