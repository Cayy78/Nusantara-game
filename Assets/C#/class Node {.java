/*
 * Kasus: Intersection kode permen dari dua gudang
 */

import java.lang.Math;

public class ArrayPermen {

    static final int MIN = 1;
    static final int MAX = 100;
    static final int JUMLAH = 25;

    // fungsi untuk generate angka random
    static void generate(int[] data) {
        for(int i = 0; i < data.length; i++) {
            data[i] = (int)(Math.random() * (MAX - MIN + 1)) + MIN;
        }
    }

    // fungsi untuk menampilkan array
    static void display(int[] data) {
        for(int i = 0; i < data.length; i++) {
            System.out.print(data[i] + " ");
        }
        System.out.println();
    }

    // fungsi untuk mencari intersection
    static void intersection(int[] A, int[] B) {
        System.out.println("\nKode permen yang sama:");

        for(int i = 0; i < A.length; i++) {
            for(int j = 0; j < B.length; j++) {
                if(A[i] == B[j]) {
                    System.out.print(A[i] + " ");
                }
            }
        }
        System.out.println();
    }

    public static void main(String[] args) {

        int[] jakarta = new int[JUMLAH];
        int[] bandung = new int[JUMLAH];

        System.out.println("Gudang Jakarta:");
        generate(jakarta);
        display(jakarta);

        System.out.println("\nGudang Bandung:");
        generate(bandung);
        display(bandung);

        intersection(jakarta, bandung);
    }
}