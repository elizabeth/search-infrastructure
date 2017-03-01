<?php
//NBA Player object
class Player implements JsonSerializable {
    private $name;
    private $team;
    private $gp;            //games played
    private $min;           //minutes avg per game
    private $fg_m;          //field goals made
    private $fg_a;          //field goals attempted
    private $fg_pct;        //field goals percentage
    private $three_pt_m;    //three pointers made
    private $three_pt_a;    //three pointers attempted
    private $three_pt_pct;  //three pointers percentage
    private $ft_m;          //free throws made
    private $ft_a;          //free throws attempted
    private $ft_pct;        //free throws percentage
    private $reb_off;       //rebounds offensive
    private $reb_def;       //rebounds defensive
    private $reb_tot;       //rebounds total
    private $misc_ast;      //assists
    private $misc_to;       //turnovers
    private $misc_stl;      //steals
    private $misc_blk;      //blocks
    private $misc_pf;       //personal fouls
    private $misc_ppg;      //points per game

    function __construct($name, $team, $gp, $min, $fg_m, $fg_a, $fg_pct, $three_pt_m, $three_pt_a, $three_pt_pct,
                         $ft_m, $ft_a, $ft_pct, $reb_off, $reb_def, $reb_tot, $ast, $to, $stl, $blk, $pf, $ppg) {
        $this->name = $name;
        $this->team = $team;
        $this->gp = $gp;
        $this->min = $min;
        $this->fg_m = $fg_m;
        $this->fg_a = $fg_a;
        $this->fg_pct = $fg_pct;
        $this->three_pt_m = $three_pt_m;
        $this->three_pt_a = $three_pt_a;
        $this->three_pt_pct = $three_pt_pct;
        $this->ft_m = $ft_m;
        $this->ft_a = $ft_a;
        $this->ft_pct = $ft_pct;
        $this->reb_off = $reb_off;
        $this->reb_def = $reb_def;
        $this->reb_tot = $reb_tot;
        $this->misc_ast = $ast;
        $this->misc_to = $to;
        $this->misc_stl = $stl;
        $this->misc_blk = $blk;
        $this->misc_pf = $pf;
        $this->misc_ppg = $ppg;
    }

    public function jsonSerialize() {
        return [
            'name' => $this->name,
            'team' => $this->team,
            'gp' => $this->gp,
            'min' => $this->format(min),
            'fg_m' => $this->format(fg_m),
            'fg_a' => $this->format(fg_a),
            'fg_pct' => $this->format(fg_pct),
            'three_pt_m' => $this->format(three_pt_m),
            'three_pt_a' => $this->format(three_pt_a),
            'three_pt_pct' => $this->format(three_pt_pct),
            'ft_m' => $this->format(ft_m),
            'ft_a' => $this->format(ft_a),
            'ft_pct' => $this->format(ft_pct),
            'reb_off' => $this->format(reb_off),
            'reb_def' => $this->format(reb_def),
            'reb_tot' => $this->reb_tot,
            'misc_ast' => $this->format(misc_ast),
            'misc_to' => $this->format(misc_to),
            'misc_stl' => $this->format(misc_stl),
            'misc_blk' => $this->format(misc_blk),
            'misc_pf' => $this->format(misc_pf),
            'misc_ppg' => $this->format(misc_ppg)
        ];
    }

    // public function getName() { return $this->name; }

    // public function getTeam() { return $this->team; }

    // public function getGp() { return $this->gp; }

    // public function getMin() { return $this->format($this->min); }

    // public function getFgM() { return $this->format($this->fg_m); }

    // public function getFgA() { return $this->format($this->fg_a); }

    // public function getFgPct() { return $this->format($this->fg_pct); }

    // public function getThreePtM() { return $this->format($this->three_pt_m); }

    // public function getThreePtA() { return $this->format($this->three_pt_a); }

    // public function getThreePtPct() { return $this->format($this->three_pt_pct); }

    // public function getFtM() { return $this->format($this->ft_m); }

    // public function getFtA() { return $this->format($this->ft_a); }

    // public function getFtPct() { return $this->format($this->ft_pct); }

    // public function getRebOff() { return $this->format($this->reb_off); }

    // public function getRebDef() { return $this->format($this->reb_def); }

    // public function getRebTot() { return $this->reb_tot; }

    // public function getAst() { return $this->format($this->misc_ast); }

    // public function getTo() { return $this->format($this->misc_to); }

    // public function getStl() { return $this->format($this->misc_stl); }

    // public function getBlk() { return $this->format($this->misc_blk); }

    // public function getPf() { return $this->format($this->misc_pf); }

    // public function getPpg() { return $this->format($this->misc_ppg); }
    
    private function format($data) {
        return isset($data) ? number_format($data, 1) : 'N/A';
    }
}