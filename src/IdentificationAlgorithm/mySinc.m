function [xymat] = mySinc(p,grid)
    %p
    %grid
    %pause(0.005);
    [n_individuals,~] = size(p);
    xymat = cell(n_individuals,1);
    for i = 1:n_individuals
        a = p(i,1);
        b = p(i,2);
        c = p(i,3);
        d = p(i,4);
        y = c*sinc(a*(grid-b)) + d*grid;
        xymat{i} = [grid y];
    end
end